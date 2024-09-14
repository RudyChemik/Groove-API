using Groove.Interfaces;
using Groove.Models.ShoppingModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System;
using Groove.VMO.Cart;
using Groove.Data;

namespace Groove.Controllers
{
    /// <summary>
    /// Controller for managing shopping cart operations
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize]
    public class ShoppingController : Controller
    {
        private readonly IShoppingService _shoppingService;
        private readonly IJwtService _jwtService;

        /// <summary>
        /// Initializes a new instance of the class
        /// </summary>
        /// <param name="shoppingService">Service for handling shopping cart operations</param>
        /// <param name="jwtService">Service for handling JWT authentication</param>
        public ShoppingController(IShoppingService shoppingService, IJwtService jwtService)
        {
            _shoppingService = shoppingService;
            _jwtService = jwtService;
        }

        /// <summary>
        /// Adds a track to the shopping cart
        /// </summary>
        /// <param name="trackId">The ID of the track to add to the cart./param>
        /// <param name="quantity">The quantity of the track to add</param>
        /// <returns>Returns Ok if the track is added successfully; otherwise, returns StatusCode 500 on error</returns>
        [HttpPost]
        public async Task<IActionResult> AddTrackToCart(int trackId, int quantity)
        {
            try
            {
                string userId = _jwtService.GetUserIdClaim();

                var cart = await _shoppingService.GetOrCreateCartAsync(userId);

                await _shoppingService.AddTrackToCart(cart, trackId, quantity);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"error: {ex.Message}");
            }
        }

        /// <summary>
        /// Adds an album to the shopping cart
        /// </summary>
        /// <param name="albumId">The ID of the album to add to the cart</param>
        /// <param name="qty">The qty of the album to add</param>
        /// <returns>Returns Ok if the album is added successfully; otherwise, returns StatusCode 500 on error</returns>
        [HttpPost]
        public async Task<IActionResult> AddAlbumToCart(int albumId, int qty)
        {
            try
            {
                string userId = _jwtService.GetUserIdClaim();

                var cart = await _shoppingService.GetOrCreateCartAsync(userId);

                bool addedToCart = await _shoppingService.AddAlbumToCart(cart, albumId, qty);

                if (addedToCart)
                {
                    return Ok();
                }

                return BadRequest();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"error: {ex.Message}");
            }
        }

        /// <summary>
        /// Removes a track from the shopping cart
        /// </summary>
        /// <param name="trackId">The ID of the track to remove</param>
        /// <returns>Returns Ok if the track is removed successfully; otherwise, returns StatusCode 500 on error</returns>
        [HttpDelete]
        public async Task<IActionResult> RemoveTrackFromCart(int trackId)
        {
            try
            {
                string userId = _jwtService.GetUserIdClaim();

                var cart = await _shoppingService.GetOrCreateCartAsync(userId);

                bool removedFromCart = await _shoppingService.RemoveTrackFromCart(cart, trackId);

                if (removedFromCart)
                {
                    return Ok();
                }

                return BadRequest();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"error: {ex.Message}");
            }
        }

        /// <summary>
        /// Removes an album from the shopping cart
        /// </summary>
        /// <param name="albumId">The ID of the album to remove</param>
        /// <returns>Returns Ok if the album is removed successfully; otherwise, returns StatusCode 500 on error</returns>
        [HttpDelete]
        public async Task<IActionResult> RemoveAlbumFromCart(int albumId)
        {
            try
            {
                string userId = _jwtService.GetUserIdClaim();

                var cart = await _shoppingService.GetOrCreateCartAsync(userId);

                bool removedFromCart = await _shoppingService.RemoveAlbumFromCart(cart, albumId);

                if (removedFromCart)
                {
                    return Ok();
                }

                return BadRequest();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"error: {ex.Message}");
            }
        }

        /// <summary>
        /// Decreases the qty of a track in the shopping cart
        /// </summary>
        /// <param name="trackId">The ID of the track to decrease</param>
        /// <returns>Returns Ok if the qty is decreased successfully; otherwise, returns StatusCode 500 on error</returns>
        [HttpDelete]
        public async Task<IActionResult> DecreaseTrack(int trackId)
        {
            try
            {
                string userId = _jwtService.GetUserIdClaim();

                var cart = await _shoppingService.GetOrCreateCartAsync(userId);

                bool decreased = await _shoppingService.DecreaseTrack(cart, trackId);

                if (decreased)
                {
                    return Ok();
                }

                return BadRequest();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"error: {ex.Message}");
            }
        }

        /// <summary>
        /// Increases the quantity of a track in the shopping cart
        /// </summary>
        /// <param name="trackId">The ID of the track to increase</param>
        /// <returns>Returns Ok if the quantity is increased successfully; otherwise, returns StatusCode 500 on error</returns>
        [HttpPost]
        public async Task<IActionResult> IncreseTrack(int trackId)
        {
            try
            {
                string userId = _jwtService.GetUserIdClaim();

                var cart = await _shoppingService.GetOrCreateCartAsync(userId);

                bool increased = await _shoppingService.IncreaseTrack(cart, trackId);

                if (increased)
                {
                    return Ok();
                }

                return BadRequest();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"error: {ex.Message}");
            }
        }

        /// <summary>
        /// Decreases the quantity of an album in the shopping cart
        /// </summary>
        /// <param name="albumId">The ID of the album to decrease</param>
        /// <returns>Returns Ok if the quantity is decreased successfully; otherwise, returns StatusCode 500 on error</returns>
        [HttpDelete]
        public async Task<IActionResult> DecreaseAlbum(int albumId)
        {
            try
            {
                string userId = _jwtService.GetUserIdClaim();

                var cart = await _shoppingService.GetOrCreateCartAsync(userId);

                bool decreased = await _shoppingService.DecreaseAlbum(cart, albumId);

                if (decreased)
                {
                    return Ok();
                }

                return BadRequest();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"error: {ex.Message}");
            }
        }

        /// <summary>
        /// Increases the quantity of an album in the shopping cart
        /// </summary>
        /// <param name="albumId">The ID of the album to increase</param>
        /// <returns>Returns Ok if the quantity is increased successfully; otherwise, returns StatusCode 500 on error</returns>
        [HttpPost]
        public async Task<IActionResult> IncreaseAlbum(int albumId)
        {
            try
            {
                string userId = _jwtService.GetUserIdClaim();

                var cart = await _shoppingService.GetOrCreateCartAsync(userId);

                bool increased = await _shoppingService.IncreaseAlbum(cart, albumId);

                if (increased)
                {
                    return Ok();
                }

                return BadRequest();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"error: {ex.Message}");
            }
        }

        /// <summary>
        /// Retrieves the total price of the items in the shopping cart
        /// </summary>
        /// <returns>The total price of the items in the cart</returns>
        [HttpGet]
        public async Task<double> GetCartPrice()
        {
            try
            {
                string userId = _jwtService.GetUserIdClaim();

                var cart = await _shoppingService.GetOrCreateCartAsync(userId);

                return _shoppingService.GetPricing(cart);
            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        /// <summary>
        /// Retrieves the shopping cart for the current user
        /// </summary>
        /// <returns>The shopping cart of the user</returns>
        [HttpGet]
        public async Task<ActionResult<CartVMO>> GetCart()
        {
            try
            {
                string userId = _jwtService.GetUserIdClaim();

                var res = await _shoppingService.ReturnCart(userId);

                if (res == null || !res.Any())
                {
                    return NotFound("empty");
                }

                return Ok(res);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"error: {ex.Message}");
            }
        }

        /// <summary>
        /// Pays for the items in the shopping cart using the user's balance
        /// </summary>
        /// <returns>Returns Ok if the payment is successful; otherwise, returns BadRequest or StatusCode 500 on error</returns>
        [HttpPost]
        public async Task<IActionResult> PayForCartByBalance()
        {
            try
            {
                string userId = _jwtService.GetUserIdClaim();

                var cart = await _shoppingService.GetShoppingCartByUserId(userId);
                if (cart == null) { return BadRequest(); }

                double price = _shoppingService.GetPricing(cart);

                var cred = await _shoppingService.DoesUserHaveCredits(userId, price);
                if (!cred) { return BadRequest(); }

                var res = await _shoppingService.ProceedOrder(userId, price, cart);
                if (!res) { return BadRequest(); }

                await _shoppingService.DeleteShoppingCartByUserId(userId);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"error: {ex.Message}");
            }
        }
    }
}
