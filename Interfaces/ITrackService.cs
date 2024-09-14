using Groove.Models;
using Groove.VMO.Track;
using Microsoft.AspNetCore.Mvc;

namespace Groove.Interfaces
{
    public interface ITrackService
    {
        Task<ActionResult<IEnumerable<GetAllTracksVM>>> GetAllTracks();
        Task<Track> GetTrackById(int userid);
        Task<List<Track>> GetTrackByArtistId(int artistId);
        Task<List<Track>> GetTrackByAlbumId(int albumId);
        Task<List<PaidTracks>> GetPaidTracks();        
        Task<List<PaidTracks>> GetPaidTrackByArtistId(int artistId);
        Task<List<PaidTracks>> GetPaidTrackByStudio(int studioId);
        Task<List<Track>> GetTrackByStudioId(int studioId);
        Task<PaidTracks> GetPaidTrackById(int id);
    }
}
