using Groove.Data;
using Groove.Interfaces;
using Groove.Models;
using Groove.Models.Order;
using Groove.Models.ShoppingModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Groove.Services
{
    public class PayPalService:IPayPalService
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        public PayPalService( AppDbContext appDbContext, UserManager<AppUser> userManager)
        {
            _context = appDbContext;
            _userManager = userManager;
        }



        public async Task<bool> PayForCart(string orderId, string userId, double cost, ShoppingCart cart)
        {
            var orderHistory = new OrderHistory
            {
                UserId = userId,
                OrderNumber = orderId,
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

            await _context.SaveChangesAsync();

            return true;
        }




        public async Task<bool> ConfirmPaymentForCart(string orderId)
        {
            var order = await _context.OrderHistory.FirstOrDefaultAsync(a=>a.OrderNumber == orderId);
            if(order == null) { return false; }

            order.isPies= true;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> AddUserBalance(string userId, double amount)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return false;
            }

            user.Balance = user.Balance + amount;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
