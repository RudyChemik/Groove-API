using Groove.Models.ShoppingModels;
using Groove.VMO.Cart;

namespace Groove.Interfaces
{
    public interface IShoppingService
    {
        Task<bool> AddTrackToCart(ShoppingCart cart, int trackId, int qty);
        Task<ShoppingCart> GetOrCreateCartAsync(string userId);
        Task<bool> AddAlbumToCart(ShoppingCart cart, int albumId, int qty);
        Task<bool> RemoveTrackFromCart(ShoppingCart cart, int trackId);
        Task<bool> RemoveAlbumFromCart(ShoppingCart cart, int albumId);
        Task<bool> DecreaseTrack(ShoppingCart cart, int trackId);
        Task<bool> IncreaseTrack(ShoppingCart cart, int trackId);
        Task<bool> DecreaseAlbum(ShoppingCart cart, int trackId);
        Task<bool> IncreaseAlbum(ShoppingCart cart, int albumId);
        double GetPricing(ShoppingCart cart);
        Task<ShoppingCart> GetShoppingCartByUserId(string userId);
        Task<bool> DeleteShoppingCartByUserId(string userId);
        Task<bool> DoesUserHaveCart(string userId);
        Task<bool> DoesUserHaveCredits(string userId, double price);
        Task<bool> ProceedOrder(string userId, double cost, ShoppingCart cart);
        Task<List<CartVMO>> ReturnCart(string userId);
    }
}
