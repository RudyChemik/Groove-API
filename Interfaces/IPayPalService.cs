using Groove.Models.ShoppingModels;

namespace Groove.Interfaces
{
    public interface IPayPalService
    {
        Task<bool> PayForCart(string orderId, string userId, double cost, ShoppingCart cart);
        Task<bool> ConfirmPaymentForCart(string orderId);
        Task<bool> AddUserBalance(string userId, double amount);
    }
}
