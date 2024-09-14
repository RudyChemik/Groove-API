using Groove.Models;
using Microsoft.AspNetCore.Mvc;

namespace Groove.Interfaces
{
    public interface IUserService
    {
        Task<List<Track>> GetAllUserLikedTracks(string userId);
        Task<List<Album>> GetAllUserLikedAlbums(string userId);

        //TO DO EXTEND WH +++ 
        Task<bool> LikeTrack(string userId, int trackId);
        Task<bool> LikeAlbum(string userId, int albumId);
        Task<bool> DislikeTrack(string userId, int trackId);
        Task<bool> DislikeAlbum(string userId, int albumId);

        Task<AppUser> GetUserWithInformation(string userId);
        Task<List<OrderHistory>> GetUserOrders(string userId);
        Task<OrderHistory> GetOrderHistoryById(int id, string userId);
        Task<bool> HasUserPurchasedTrack(string userId, int trackId);
        Task<List<PaidTracks>> GetPurchasedTracksByUser(string userId);
        Task<double> GetUserBalance(string userId);
    }
}
