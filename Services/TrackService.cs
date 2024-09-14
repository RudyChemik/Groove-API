using Groove.Data;
using Groove.Interfaces;
using Groove.Models;
using Groove.VMO.Track;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Groove.Services
{
    public class TrackService : ITrackService
    {
        private readonly AppDbContext _context;
        public TrackService(AppDbContext appDbContext)
        {
            _context = appDbContext;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<GetAllTracksVM>>> GetAllTracks()
        {
            var tracks = await _context.Track.Include(a => a.Album).ToListAsync();

            var viewmodels = tracks.Select(track => new GetAllTracksVM
            {
                Id = track.Id,
                Name = track.Name,
                Img = track.Img,
                BlobUrl = track.BlobUrl,
                ArtistId = track.ArtistId,
                AlbumId = track.AlbumId,
                StudioId = track.StudioId,
                ArtistName = track.Artist?.Name,
                AlbumName = track.Album?.Name,   
                StudioName = track.Studio?.Name   
            }).ToList();

            return viewmodels;
        }

        public async Task<Track> GetTrackById(int id)
        {
            var res = await _context.Track.FindAsync(id);
            return res;
        }

        public async Task<List<Track>> GetTrackByArtistId(int ArtistId)
        {
            var res = await _context.Track.Where(a => a.ArtistId == ArtistId).ToListAsync();
            return res;

        }
        public async Task<List<Track>> GetTrackByAlbumId(int AlbumId)
        {
            var res = await _context.Track.Include(a => a.Album).Where(a => a.AlbumId == AlbumId).ToListAsync();
            return res;
        }
        public async Task<List<Track>> GetTrackByStudioId(int StudioId)
        {
            var res = await _context.Track.Where(a => a.StudioId == StudioId).ToListAsync();
            return res;
        }

        public async Task<PaidTracks> GetPaidTrackById(int id)
        {
            var res = await _context.PaidTracks.Where(a => a.Id == id).FirstOrDefaultAsync();
            return res;
        }

        public async Task<List<PaidTracks>> GetPaidTracks()
        {
            var res = await _context.PaidTracks.Where(a => a.IsVisible == true).ToListAsync();
            return res;
        }

        public async Task<List<PaidTracks>> GetPaidTrackByArtistId(int artistId)
        {
            var res = await _context.PaidTracks.Where(a => a.ArtistId == artistId && a.IsVisible == true).ToListAsync();
            return res;
        }

        public async Task<List<PaidTracks>> GetPaidTrackByStudio(int studioId)
        {
            var res = await _context.PaidTracks.Where(a => a.StudioId == studioId && a.IsVisible == true).ToListAsync();
            return res;
        }
    }

            
    
}
