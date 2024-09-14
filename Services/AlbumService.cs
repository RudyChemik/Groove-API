using Groove.Data;
using Groove.Interfaces;
using Groove.Models;
using Groove.VMO.Album;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Groove.Services
{
    public class AlbumService : IAlbumService
    {
        private readonly AppDbContext _context;
        public AlbumService(AppDbContext appDbContext)
        {
            _context = appDbContext;
        }

        public async Task<Album> GetAlbumById(int id)
        {
            var res = await _context.Album
                .Include(a => a.Tracks)
                .SingleOrDefaultAsync(a => a.Id == id);

            return res;
        }


        public async Task<ActionResult<IEnumerable<Album>>> GetAllAlbums()
        {
            var albums = await _context.Album.ToListAsync();
            return albums;
        }

        public async Task<ActionResult<IEnumerable<AllAlbumsVMO>>> GetAllAlbumAlongWithAS()
        {
            var albums = await _context.Album.Include(c => c.Artist).Include(s => s.Studio).ToListAsync();
            var allAlbumsVMOs = albums.Select(album => new AllAlbumsVMO
            {
                AlbumId = album.Id,
                Title = album.Name,
                Description = album.Desc,
                AlbumName = album.Name,
                StudioName = album.Studio?.Name,
                Tracks = album.Tracks
            }).ToList();

            return allAlbumsVMOs;
        }


        public async Task<ActionResult<IEnumerable<PaidAlbums>>> GetAllPaid()
        {
            var res = await _context.PaidAlbums.Where(a=>a.IsVisible == true).ToListAsync();

            return res;
        }
        public async Task<List<PaidAlbums>> GetPurchasedAlbumsByUser(string userId)
        {
            var purchasedAlbums = await _context.OrderHistory
                .Where(order => order.UserId == userId)
                .SelectMany(order => order.PurchasedAlbums)
                .Where(album => album.PaidAlbum != null)
                .Select(album => album.PaidAlbum)
                .ToListAsync();

            return purchasedAlbums;
        }

        public async Task<List<PaidAlbumTrack>> GetPurchasedAlbumTracks(int albumId)
        {
            var albumTracks = await _context.PaidAlbumTracks
                .Where(t => t.PaidAlbumId == albumId)
                .ToListAsync();

            return albumTracks;
        }

        public async Task<bool> HasUserPurchasedAlbum(string userId, int albumId)
        {
            var hasPurchasedAlbum = await _context.OrderHistory
            .AnyAsync(order => order.UserId == userId && order.PurchasedAlbums.Any(album => album.PaidAlbumId == albumId));

            return hasPurchasedAlbum;
        }
        public async Task<List<Album>> GetAlbumByArtistId(int ArtistId)
        {
            var res = await _context.Album.Where(a => a.ArtistId == ArtistId).ToListAsync();
            return res;
        }
        public async Task<List<Album>> GetAlbumByStudioId(int StudioId)
        {
            var res = await _context.Album.Where(a => a.StudioId == StudioId).ToListAsync();
            return res;
        }
    }
}
