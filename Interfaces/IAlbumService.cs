using Groove.Models;
using Microsoft.AspNetCore.Mvc;

namespace Groove.Interfaces
{
    public interface IAlbumService
    {
        Task<ActionResult<IEnumerable<Album>>> GetAllAlbums();
        Task<Album> GetAlbumById(int id);
        Task<ActionResult<IEnumerable<PaidAlbums>>> GetAllPaid();
        Task<List<PaidAlbums>> GetPurchasedAlbumsByUser(string userId);
        Task<bool> HasUserPurchasedAlbum(string userId, int albumId);
        Task<List<PaidAlbumTrack>> GetPurchasedAlbumTracks(int albumId);
        Task<List<Album>> GetAlbumByArtistId(int artistId);
        Task<List<Album>> GetAlbumByStudioId(int studioId);
    }
}
