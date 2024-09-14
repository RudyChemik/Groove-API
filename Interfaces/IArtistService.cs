using Groove.Models;
using Groove.VM;
using Groove.VM.Artist;
using Groove.VM.Track;
using Microsoft.AspNetCore.Mvc;

namespace Groove.Interfaces
{
    public interface IArtistService
    {
        Task<ActionResult<IEnumerable<Artist>>> GetAllArtists();
        Task<bool> IsUserArtist(string userId);
        Task<bool> BecomeArtistAsync(BecomeArtistVM model, string userId);
        Task<int> GetArtistId(string userId);
        Task<bool> AddTrack(int artistId, AddTrackVM model, string blobUrl);
        Task<bool> ChangeArtistData(ChnageArtistDataVM model, int artistId);
        Task<bool> AddAlbum(int artistId, AddAlbumVM model);
        Task<bool> DeleteArtist(int artistId);
        Task<bool> DeleteTrack(string userId, int trackId);
        Task<bool> DeleteAlbum(string userId, int albumId);
        Task<bool> UpdateTrack(UpdateTrackVM model, string userId, string blobURl);
        Task<bool> AddTrackToAlbum(int trackId, int albumId, int artistId);
        Task<bool> RemoveFromAlbum(int trackId, int albumId, int artistId);
        Task<bool> ApplyForStudio(int artistId, int studioId);
        Task<bool> DoesArtistHaveAStudio(int artistId);
        Task<bool> AlreadyMadeRequest(int artistId, int studioId);
        Task<string> GetArtistIdById(int artistId);
        Task<int> GetArtistIdByAlbumId(int albumId);
        Task<int> GetArtistIdByTrackId(int trackId);
        Task<int> GetArtistIdByPaidAlbum(int albumId);
        Task<int> GetArtistIdByPaidTrackId(int trackId);
        Task<int> GetArtistIdByPaidAlbumTrackId(int trackId);
        Task<bool> CreateAlbumWithTracks(CreateAlbumWithTracksVM model, int artistId);
        Task<List<Album>> GetAlbumByUserId(string userId);
    }
}
