using Groove.Data;
using Groove.Interfaces;
using Groove.Migrations;
using Groove.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Groove.Services
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public UserService(UserManager<AppUser> userManager, AppDbContext appDbContext)
        {
            _userManager = userManager;
            _context = appDbContext;
        }
        

        public async Task<List<Album>> GetAllUserLikedAlbums(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                throw new Exception("Użytkownik nie znaleziony");
            }

            var likedAlbums = await _context.AlbumLikes
                .Where(ul => ul.AppUserId == userId)
                .Select(ul => ul.Album)
                .ToListAsync();

            return likedAlbums;
        }

        public async Task<List<Track>> GetAllUserLikedTracks(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                throw new Exception("Użytkownik nie znaleziony");
            }

            var likedTracks = await _context.UserLikes
                .Where(ul => ul.AppUserId == userId)
                .Select(ul => ul.Track)
                .ToListAsync();

            return likedTracks;
        }

        public async Task<bool> LikeAlbum(string userId, int albumId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return false;
                }

                var album = await _context.Album.FirstOrDefaultAsync(t => t.Id == albumId);
                if (album == null)
                {
                    return false;
                }

                var existingLike = await _context.AlbumLikes
                    .FirstOrDefaultAsync(ul => ul.AppUserId == userId && ul.AlbumId == albumId);

                if (existingLike != null)
                {
                    return false;
                }

                var newLike = new AlbumLike
                {
                    AppUserId = userId,
                    AlbumId = albumId
                };

                _context.AlbumLikes.Add(newLike);
                await _context.SaveChangesAsync();

                return true;

            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> LikeTrack(string userId, int trackId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return false;
                }

                var track = await _context.Track.FirstOrDefaultAsync(t => t.Id == trackId);
                if (track == null)
                {
                    return false;
                }

                var existingLike = await _context.UserLikes
                    .FirstOrDefaultAsync(ul => ul.AppUserId == userId && ul.TrackId == trackId);

                if (existingLike != null)
                {
                    return false;
                }

                var newLike = new UserLike
                {
                    AppUserId = userId,
                    TrackId = trackId
                };

                _context.UserLikes.Add(newLike);
                await _context.SaveChangesAsync();

                return true;

            }
            catch
            {
                return false;
            }
               
        }

        public async Task<bool> DislikeTrack(string userId, int trackId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return false;
                }

                var track = await _context.Track.FirstOrDefaultAsync(t => t.Id == trackId);
                if (track == null)
                {
                    return false;
                }

                var existingLike = await _context.UserLikes.FirstOrDefaultAsync(ul => ul.AppUserId == userId && ul.TrackId == trackId);

                if (existingLike == null)
                {
                    return false;
                }

                _context.UserLikes.Remove(existingLike);
                await _context.SaveChangesAsync();

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DislikeAlbum(string userId, int albumId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return false;
                }

                var album = await _context.Album.FirstOrDefaultAsync(t => t.Id == albumId);
                if (album == null)
                {
                    return false;
                }

                var existingLike = await _context.AlbumLikes.FirstOrDefaultAsync(ul => ul.AppUserId == userId && ul.AlbumId == albumId);

                if (existingLike == null)
                {
                    return false;
                }

                _context.AlbumLikes.Remove(existingLike);
                await _context.SaveChangesAsync();

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<AppUser> GetUserWithInformation(string userId)
        {
            var user = await _context.AppUser
                .Include(a => a.UserInformation)
                .FirstOrDefaultAsync(u => u.Id == userId);

            return user;
        }

        public async Task<List<OrderHistory>> GetUserOrders(string userId)
        {
            var res = await _context.OrderHistory.Where(a=>a.UserId == userId).ToListAsync();
            return res;
        }

        public async Task<OrderHistory> GetOrderHistoryById(int id, string userId)
        {

            var orderHistory = await _context.OrderHistory
            .Include(o => o.PurchasedItems)
                .ThenInclude(item => item.PaidTrack)
                .ThenInclude(t => t.Artist)
                .ThenInclude(t => t.Studio)
            .Include(o => o.PurchasedAlbums)
                .ThenInclude(album => album.PaidAlbum) 
                .ThenInclude(a=>a.Tracks)
            .FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId);


            return orderHistory;
        }

        public async Task<bool> HasUserPurchasedTrack(string userId, int trackId)
        {
            var hasPurchased = await _context.OrderHistory
                .AnyAsync(order => order.UserId == userId && order.PurchasedItems.Any(item => item.PaidTrackId == trackId));

            return hasPurchased;
        }

        public async Task<List<PaidTracks>> GetPurchasedTracksByUser(string userId)
        {
            var purchasedTracks = await _context.OrderHistory
                .Where(order => order.UserId == userId)
                .SelectMany(order => order.PurchasedItems)
                .Where(item => item.PaidTrack != null)
                .Select(item => item.PaidTrack)
                .ToListAsync();

            return purchasedTracks;
        }

        public async Task<double> GetUserBalance(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user != null)
            {
                return user.Balance;
            }

            return 0;
        }


    }
}
