using Groove.Interfaces;
using Groove.Services;
using Groove.VM.PayPal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using PayPal.Api;
using PayPalCheckoutSdk.Core;
using PayPalCheckoutSdk.Orders;
using PayPalHttp;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using static Groove.VM.PayPal.PayPalWebhookVM;

/// <summary>
/// Controller for handling PayPal
/// </summary>
[Route("api/paypal")]
[ApiController]
public class PayPalController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly PayPalEnvironment _paypalEnvironment;
    private readonly IPayPalService _payPalService;
    private readonly IShoppingService _shoppingService;

    /// <summary>
    /// Initializes a new instance of the class
    /// </summary>
    /// <param name="configuration">The configuration settings</param>
    /// <param name="paypal">The PayPal environment</param>
    /// <param name="payPalService">Service for handling PayPal-specific operations</param>
    /// <param name="shoppingService">Service for managing shopping cart operations</param>
    public PayPalController(IConfiguration configuration, PayPalEnvironment paypal, IPayPalService payPalService, IShoppingService shoppingService)
    {
        _configuration = configuration;
        _paypalEnvironment = paypal;
        _payPalService = payPalService;
        _shoppingService = shoppingService;
    }

    /// <summary>
    /// Processes a payment for the current user's shopping cart using PayPal
    /// </summary>
    /// <returns>Returns an order ID and approval URL if successful; otherwise, returns BadRequest or StatusCode 500 on error</returns>
    [HttpPost("cart")]
    public async Task<IActionResult> ProcessPaymentCart()
    {
        try
        {
            string userId = "f7342d70-37d9-4df0-8719-6ab5996acaa1"; //just for tyhe tests

            var cart = await _shoppingService.GetShoppingCartByUserId(userId);
            if (cart == null) { return BadRequest("The cart is empty."); }

            var price = _shoppingService.GetPricing(cart);
            if (cart == null) { return BadRequest("Error in pricing calculation."); }

            var payPalClientId = _configuration["PayPalSettings:ClientId"];
            var payPalClientSecret = _configuration["PayPalSettings:ClientSecret"];

            var environment = new SandboxEnvironment(payPalClientId, payPalClientSecret);
            var client = new PayPalHttpClient(environment);

            var createOrder = new OrderRequest()
            {
                CheckoutPaymentIntent = "CAPTURE",
                PurchaseUnits = new List<PurchaseUnitRequest>
                {
                    new PurchaseUnitRequest
                    {
                        AmountWithBreakdown = new AmountWithBreakdown
                        {
                            CurrencyCode = "PLN",
                            Value = price.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture)
                        },
                        CustomId = $"CartPayment:{price}:{userId}"
                    }
                }
            };

            var createOrderRequest = new OrdersCreateRequest();
            createOrderRequest.Prefer("return=representation");
            createOrderRequest.RequestBody(createOrder);

            var createOrderResponse = await client.Execute(createOrderRequest);

            if ((int)createOrderResponse.StatusCode == 201)
            {
                var orderResponse = createOrderResponse.Result<PayPalCheckoutSdk.Orders.Order>();
                var orderId = orderResponse.Id;

                var getOrderRequest = new OrdersGetRequest(orderId);
                var getOrderResponse = await client.Execute(getOrderRequest);

                if ((int)getOrderResponse.StatusCode == 200)
                {
                    var orderDetails = getOrderResponse.Result<PayPalCheckoutSdk.Orders.Order>();
                    var approvalUrl = orderDetails.Links.FirstOrDefault(link => link.Rel == "approve");

                    if (approvalUrl != null)
                    {
                        await _payPalService.PayForCart(orderId, userId, price, cart);
                        return Ok(new { OrderId = orderId, ApprovalUrl = approvalUrl.Href });
                    }
                }
            }

            return BadRequest("Failed to create or initiate PayPal payment.");
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Internal Server Error");
        }
    }

    /// <summary>
    /// Adds balance to the user's account using PayPal
    /// </summary>
    /// <param name="amount">The amount to be added to the user's balance</param>
    /// <returns>Returns an order ID and approval URL if successful; otherwise, returns BadRequest or StatusCode 500 on error</returns>
    [HttpPost("balance")]
    public async Task<IActionResult> AddBalanceToAccount(double amount)
    {
        try
        {
            string userId = "f7342d70-37d9-4df0-8719-6ab5996acaa1";

            var payPalClientId = _configuration["PayPalSettings:ClientId"];
            var payPalClientSecret = _configuration["PayPalSettings:ClientSecret"];

            var environment = new SandboxEnvironment(payPalClientId, payPalClientSecret);
            var client = new PayPalHttpClient(environment);

            var createOrder = new OrderRequest()
            {
                CheckoutPaymentIntent = "CAPTURE",
                PurchaseUnits = new List<PurchaseUnitRequest>
                {
                    new PurchaseUnitRequest
                    {
                        AmountWithBreakdown = new AmountWithBreakdown
                        {
                            CurrencyCode = "PLN",
                            Value = amount.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture)
                        },
                        CustomId = $"AddBalance:{amount}:{userId}"
                    }
                }
            };

            var createOrderRequest = new OrdersCreateRequest();
            createOrderRequest.Prefer("return=representation");
            createOrderRequest.RequestBody(createOrder);

            var createOrderResponse = await client.Execute(createOrderRequest);

            if ((int)createOrderResponse.StatusCode == 201)
            {
                var orderResponse = createOrderResponse.Result<PayPalCheckoutSdk.Orders.Order>();
                var orderId = orderResponse.Id;

                var getOrderRequest = new OrdersGetRequest(orderId);
                var getOrderResponse = await client.Execute(getOrderRequest);

                if ((int)getOrderResponse.StatusCode == 200)
                {
                    var orderDetails = getOrderResponse.Result<PayPalCheckoutSdk.Orders.Order>();
                    var approvalUrl = orderDetails.Links.FirstOrDefault(link => link.Rel == "approve");

                    if (approvalUrl != null)
                    {
                        return Ok(new { OrderId = orderId, ApprovalUrl = approvalUrl.Href });
                    }
                }
            }

            return BadRequest("Failed to create or initiate PayPal payment.");
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Internal Server Error");
        }
    }

    /// <summary>
    /// Handles PayPal webhook events to process different types of events such as payment captures and refunds
    /// </summary>
    /// <returns>Returns Ok if the webhook is successfully processed; otherwise, returns BadRequest or Unauthorized on error</returns>
    [HttpPost("webhook")]
    [Consumes("application/json")]
    public async Task<IActionResult> PayPalWebhook()
    {
        string requestBody;
        using (var reader = new StreamReader(Request.Body, Encoding.UTF8))
        {
            requestBody = await reader.ReadToEndAsync();
        }

        var headers = GetPayPalHeaders(Request.Headers);

        var webhookEvent = JsonConvert.DeserializeObject<PayPalWebhookVM>(requestBody);
        if (webhookEvent == null)
        {
            return BadRequest("Invalid webhook notification.");
        }

        string orderId;
        string customId = "";
        switch (webhookEvent.event_type)
        {
            case "PAYMENT.CAPTURE.COMPLETED":
                break;

            case "PAYMENT.CAPTURE.REFUNDED":
                break;

            case "CHECKOUT.ORDER.APPROVED":
                if (webhookEvent.resource?.purchase_units != null)
                {
                    customId = webhookEvent.resource.purchase_units.FirstOrDefault()?.custom_id;
                }
                if (!string.IsNullOrEmpty(customId))
                {
                    var customIdParts = customId.Split(':');
                    if (customIdParts.Length >= 3)
                    {
                        if (customIdParts[0] == "AddBalance")
                        {
                            double amount = double.Parse(customIdParts[1]);
                            string userId = customIdParts[2];

                            orderId = webhookEvent.resource.id;
                            await CapturePayment(orderId);
                            await _payPalService.AddUserBalance(userId, amount);

                            break;
                        }
                        else
                        {
                            string userId = customIdParts[2];

                            orderId = webhookEvent.resource.id;
                            await CapturePayment(orderId);
                            await _payPalService.ConfirmPaymentForCart(orderId);
                            await _shoppingService.DeleteShoppingCartByUserId(userId);

                            break;
                        }
                    }
                }

                orderId = webhookEvent.resource.id;
                await CapturePayment(orderId);
                await _payPalService.ConfirmPaymentForCart(orderId);
                break;

            default:
                return BadRequest("Unknown event type.");
        }

        return Ok();
    }

    /// <summary>
    /// Extracts PayPal headers from the HTTP request for verification purposes
    /// </summary>
    /// <param name="headers">The HTTP headers from the request</param>
    /// <returns>A dictionary containing the relevant PayPal headers</returns>
    private Dictionary<string, string> GetPayPalHeaders(Microsoft.AspNetCore.Http.IHeaderDictionary headers)
    {
        return new Dictionary<string, string>
        {
            { "PayPal-Transmission-Id", headers["PayPal-Transmission-Id"] },
            { "PayPal-Transmission-Time", headers["PayPal-Transmission-Time"] },
            { "PayPal-Transmission-Sig", headers["PayPal-Transmission-Sig"] },
            { "PayPal-Cert-Url", headers["PayPal-Cert-Url"] },
            { "PayPal-Auth-Algo", headers["PayPal-Auth-Algo"] },
            { "PayPal-Cert-Id", headers["PayPal-Cert-Id"] }
        };
    }

    /// <summary>
    /// Verifies the webhook signature to ensure the authenticity of the PayPal webhook event
    /// </summary>
    /// <param name="headers">The headers received in the webhook event</param>
    /// <param name="requestBody">The body of the webhook event</param>
    /// <returns>A boolean indicating whether the webhook signature is verified</returns>
    private async Task<bool> VerifyWebhookSignature(Dictionary<string, string> headers, string requestBody)
    {
        var verificationRequestBody = new
        {
            auth_algo = headers["PayPal-Auth-Algo"],
            cert_url = headers["PayPal-Cert-Url"],
            transmission_id = headers["PayPal-Transmission-Id"],
            transmission_sig = headers["PayPal-Transmission-Sig"],
            transmission_time = headers["PayPal-Transmission-Time"],
            webhook_id = _configuration["PayPalSettings:WebhookId"],
            webhook_event = requestBody
        };

        var client = new System.Net.Http.HttpClient();
        var verifyRequest = new HttpRequestMessage(HttpMethod.Post, _paypalEnvironment.BaseUrl + "/v1/notifications/verify-webhook-signature")
        {
            Content = new StringContent(JsonConvert.SerializeObject(verificationRequestBody), Encoding.UTF8, "application/json")
        };

        try
        {
            var response = await client.SendAsync(verifyRequest);
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                dynamic verificationResponse = JsonConvert.DeserializeObject(responseContent);
                return verificationResponse.verification_status == "SUCCESS";
            }
        }
        catch (Exception ex)
        {
            return false;
        }

        return false;
    }

    /// <summary>
    /// Captures a payment for a given order ID
    /// </summary>
    /// <param name="orderId">The ID of the PayPal order to capture</param>
    /// <returns>Returns Ok with the order ID and status if successful; otherwise, returns BadRequest on error</returns>
    private async Task<IActionResult> CapturePayment(string orderId)
    {
        try
        {
            var payPalClientId = _configuration["PayPalSettings:ClientId"];
            var payPalClientSecret = _configuration["PayPalSettings:ClientSecret"];

            var environment = new SandboxEnvironment(payPalClientId, payPalClientSecret);
            var client = new PayPalHttpClient(environment);

            var captureOrderRequest = new OrdersCaptureRequest(orderId);
            captureOrderRequest.RequestBody(new OrderActionRequest());
            captureOrderRequest.Prefer("return=representation");

            var captureOrderResponse = await client.Execute(captureOrderRequest);
            var captureOrderResult = captureOrderResponse.Result<PayPalCheckoutSdk.Orders.Order>();

            if (captureOrderResult != null && captureOrderResult.Status.Equals("COMPLETED"))
            {
                return Ok(new { OrderId = captureOrderResult.Id, Status = captureOrderResult.Status });
            }
            else
            {
                return BadRequest();
            }
        }
        catch (Exception ex)
        {
            return BadRequest();
        }
    }
}
