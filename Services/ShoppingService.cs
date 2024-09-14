using Groove.Data;
using Groove.Interfaces;
using Groove.Models;
using Groove.Models.Order;
using Groove.Models.ShoppingModels;
using Groove.VMO.Cart;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;

namespace Groove.Services
{
    public class ShoppingService:IShoppingService
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public ShoppingService(AppDbContext appDbContext, UserManager<AppUser> userManager)
        {
            _context = appDbContext;
            _userManager = userManager;
        }

        public async Task<bool> AddTrackToCart(ShoppingCart cart, int trackId, int quantity)
        {
            var existingCartItem = cart.Items.FirstOrDefault(item => item.ItemId == trackId && item.ItemType == ItemType.Track);
            var track = await _context.PaidTracks.FirstOrDefaultAsync(a => a.Id == trackId);
            if (track == null) { return false; }

            if (existingCartItem != null)
            {
                existingCartItem.Quantity += quantity;
            }
            else
            {
                var newCartItem = new ShoppingCartItem
                {
                    UserId = cart.UserId,
                    ShoppingCartId = cart.Id,
                    ItemId = trackId,
                    ItemType = ItemType.Track,
                    Quantity = quantity,
                    Price = track.Price,
                };
                cart.Items.Add(newCartItem);
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<CartVMO>> ReturnCart(string userId)
        {
            var cartReturn = new List<CartVMO>();

            var cart = await _context.ShoppingCarts.Include(c => c.Items).Where(c => c.UserId == userId).FirstOrDefaultAsync();

            if (cart == null)
            {
                return cartReturn;
            }

            foreach (var item in cart.Items)
            {
                string itemName = null;

                if (item.ItemType == ItemType.Track)
                {
                    var track = await _context.PaidTracks.FirstOrDefaultAsync(t => t.Id == item.ItemId);
                    itemName = track?.Name;
                }
                else if (item.ItemType == ItemType.Album)
                {
                    var album = await _context.PaidAlbums.FirstOrDefaultAsync(a => a.Id == item.ItemId);
                    itemName = album?.Name;
                }

                var cartItem = new CartVMO
                {
                    Quantity = item.Quantity,
                    Price = item.Price,
                    TrackName = item.ItemType == ItemType.Track ? itemName : null,
                    AlbumName = item.ItemType == ItemType.Album ? itemName : null,
                    Type = item.ItemType.ToString(),
                    ItemId = item.ItemId
                };

                cartReturn.Add(cartItem);
            }

            return cartReturn;
        }


        public async Task<ShoppingCart> GetOrCreateCartAsync(string userId)
        {
            var cart = await _context.ShoppingCarts
                .Include(c => c.Items)
                .Where(c => c.UserId == userId)
                .FirstOrDefaultAsync();

            if (cart == null)
            {
                cart = new ShoppingCart { UserId = userId };
                _context.ShoppingCarts.Add(cart);
                await _context.SaveChangesAsync();

                cart = await _context.ShoppingCarts
                .Include(c => c.Items)
                .Where(c => c.UserId == userId)
                .FirstOrDefaultAsync();

                return cart;
            }

            return cart;
        }

        public async Task<bool> AddAlbumToCart(ShoppingCart cart, int albumId, int quantity)
        {
            var existingCartItem = cart.Items.FirstOrDefault(item => item.ItemId == albumId && item.ItemType == ItemType.Album);

            var album = await _context.PaidAlbums.FirstOrDefaultAsync(a=>a.Id == albumId);
            if(album == null) { return false; }

            if (existingCartItem != null)
            {
                existingCartItem.Quantity += quantity;
            }
            else
            {
                var newCartItem = new ShoppingCartItem
                {
                    UserId = cart.UserId,
                    ShoppingCartId = cart.Id,
                    ItemId = albumId,
                    ItemType = ItemType.Album,
                    Quantity = quantity,
                    Price = album.Price
                };
                cart.Items.Add(newCartItem);
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemoveTrackFromCart(ShoppingCart cart, int trackId)
        {
            var trackItem = cart.Items.FirstOrDefault(item => item.ItemId == trackId && item.ItemType == ItemType.Track);

            if (trackItem != null)
            {
                cart.Items.Remove(trackItem);
                await _context.SaveChangesAsync();
                return true;
            }

            return false;
        }

        public async Task<bool> RemoveAlbumFromCart(ShoppingCart cart, int albumId)
        {
            var album = cart.Items.FirstOrDefault(item => item.ItemId == albumId && item.ItemType == ItemType.Album);

            if (album != null)
            {
                cart.Items.Remove(album);
                await _context.SaveChangesAsync();
                return true;
            }

            return false;
        }

        public async Task<bool> DecreaseTrack(ShoppingCart cart, int trackId)
        {
            var trackItem = cart.Items.FirstOrDefault(item => item.ItemId == trackId && item.ItemType == ItemType.Track);

            if (trackItem != null)
            {
                if (trackItem.Quantity > 1)
                {
                    trackItem.Quantity -= 1;
                }
                else
                {
                    cart.Items.Remove(trackItem);
                }

                await _context.SaveChangesAsync();
                return true;
            }

            return false;
        }

        public async Task<bool> IncreaseTrack(ShoppingCart cart, int trackId)
        {
            var trackItem = cart.Items.FirstOrDefault(item => item.ItemId == trackId && item.ItemType == ItemType.Track);

            if (trackItem != null)
            {
                trackItem.Quantity += 1;

                await _context.SaveChangesAsync();
                return true;
            }

            return false;
        }

        public async Task<bool> DecreaseAlbum(ShoppingCart cart, int albumId)
        {
            var album = cart.Items.FirstOrDefault(item => item.ItemId == albumId && item.ItemType == ItemType.Album);

            if (album != null)
            {
                if (album.Quantity > 1)
                {
                    album.Quantity -= 1;
                }
                else
                {
                    cart.Items.Remove(album);
                }

                await _context.SaveChangesAsync();
                return true;
            }

            return false;
        }

        public async Task<bool> IncreaseAlbum(ShoppingCart cart, int albumId)
        {
            var album = cart.Items.FirstOrDefault(item => item.ItemId == albumId && item.ItemType == ItemType.Album);

            if(album != null)
            {
                album.Quantity += 1;

                await _context.SaveChangesAsync();
                return true;
            }

            return false;
        }

        public async Task<ShoppingCart> GetShoppingCartByUserId(string userId)
        {
            var cart = await _context.ShoppingCarts
                .Include(c => c.Items)
                .Where(c => c.UserId == userId)
                .FirstOrDefaultAsync();

            return cart;
        }


        public double GetPricing(ShoppingCart cart)
        {
            double totalPrice = 0.0;

            foreach (var cartItem in cart.Items)
            {
                double itemSubtotal = cartItem.Price * cartItem.Quantity;
                totalPrice += itemSubtotal;
            }

            return totalPrice;
        }

        public async Task<bool> DeleteShoppingCartByUserId(string userId)
        {
            var cart = await _context.ShoppingCarts
                .Include(c => c.Items)
                .Where(c => c.UserId == userId)
                .FirstOrDefaultAsync();

            if (cart == null)
            {
                return false;
            }

            _context.ShoppingCartItems.RemoveRange(cart.Items);
            _context.ShoppingCarts.Remove(cart);

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DoesUserHaveCart(string userId)
        {
            var res = await _context.ShoppingCarts.FirstOrDefaultAsync(a=>a.UserId == userId);
            if(res != null) { return true; } return false;
        }

        public async Task<bool> DoesUserHaveCredits(string userId, double price)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) { return false;}

            if(user.Balance >= price) { return true; } return false;

        }

        public async Task<bool> ProceedOrder(string userId, double cost, ShoppingCart cart)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) { return false; }

            var orderHistory = new OrderHistory
            {
                UserId = userId,
                OrderNumber = Guid.NewGuid().ToString("N").Substring(0, 16),
                OrderCost = cost,
                isPies = false,
                OrderDate = DateTime.UtcNow
            };

            _context.OrderHistory.Add(orderHistory);
            await _context.SaveChangesAsync();

            foreach (var cartItem in cart.Items)
            {
                if (cartItem.ItemType == ItemType.Track)
                {
                    var existingTrackOrder = _context.OrderedTrack.FirstOrDefault(t =>
                        t.OrderHistoryId == orderHistory.Id && t.PaidTrackId == cartItem.ItemId);

                    if (existingTrackOrder != null)
                    {
                        existingTrackOrder.qty += cartItem.Quantity;
                    }
                    else
                    {
                        var orderedTrack = new OrderedTrack
                        {
                            OrderHistoryId = orderHistory.Id,
                            PaidTrackId = cartItem.ItemId,
                            qty = cartItem.Quantity
                        };
                        _context.OrderedTrack.Add(orderedTrack);
                    }
                }
                else if (cartItem.ItemType == ItemType.Album)
                {
                    var existingAlbumOrder = _context.OrderedAlbums.FirstOrDefault(a =>
                        a.OrderHistoryId == orderHistory.Id && a.PaidAlbumId == cartItem.ItemId);

                    if (existingAlbumOrder != null)
                    {
                        existingAlbumOrder.qty += cartItem.Quantity;
                    }
                    else
                    {
                        var orderedAlbum = new OrderedAlbums
                        {
                            OrderHistoryId = orderHistory.Id,
                            PaidAlbumId = cartItem.ItemId,
                            qty = cartItem.Quantity
                        };
                        _context.OrderedAlbums.Add(orderedAlbum);
                    }
                }
            }

            orderHistory.isPies = true;
            user.Balance = user.Balance - cost;
            await _context.SaveChangesAsync();

            return true;
        }

    }
}
