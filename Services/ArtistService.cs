using Groove.Data;
using Groove.Interfaces;
using Groove.Models;
using Groove.VM;
using Groove.VM.Artist;
using Groove.VM.Track;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Groove.Services
{
    public class ArtistService : IArtistService
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly IBlobService _blobService;
        public ArtistService(AppDbContext appDbContext, UserManager<AppUser> userManager, IBlobService blobService)
        {
            _userManager = userManager;
            _context = appDbContext;
            _blobService = blobService;
        }

        public async Task<bool> AddAlbum(int artistId, AddAlbumVM model)
        {
            var artist = await _context.Artists.FindAsync(artistId);
            if (artist == null)
            {
                return false;
            }

            var album = new Album
            {
                Name = model.Name,
                Desc = model.Desc,
                Img = model.Img,
                ArtistId = artistId,
            };

            _context.Album.Add(album);
            await _context.SaveChangesAsync();

            return true;

        }

        public async Task<bool> AddTrack(int artistId, AddTrackVM model, string blobUrl)
        {
            var artist = await _context.Artists.FindAsync(artistId);
            if (artist == null)
            {
                return false;
            }

            if(model.AlbumId == 0)
            {
                var track = new Track
                {
                    Name = model.Name,
                    Img = model.Img,
                    ArtistId = artistId,
                    BlobUrl = blobUrl
                };
                _context.Track.Add(track);
            }
            else
            {
                var track = new Track
                {
                    Name = model.Name,
                    Img = model.Img,
                    AlbumId = model.AlbumId,
                    ArtistId = artistId,
                    BlobUrl = blobUrl
                };
                _context.Track.Add(track);
            }


            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> BecomeArtistAsync(BecomeArtistVM model, string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return false;
            }

            await _userManager.AddToRoleAsync(user, "artist");

            var artist = new Artist
            {
                UserId = userId,
                Name = model.Name,
                Img = model.Img,
                Desc = model.Desc
            };

            _context.Artists.Add(artist);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ChangeArtistData(ChnageArtistDataVM model, int artistId)
        {
            var artist = await _context.Artists.FindAsync(artistId);
            if (artist != null)
            {
                artist.Name = model.Name;
                artist.Desc = model.Desc;
                artist.Img = model.Img;
                await _context.SaveChangesAsync();
                return true;
            }
            else
            {
                return false;
            }

        }

        public async Task<bool> DeleteArtist(int artistId)
        {
            var artist = await _context.Artists
                .Include(a => a.Tracks)
                .Include(a => a.Albums)
                .FirstOrDefaultAsync(a => a.Id == artistId);

            if (artist == null)
            {
                return false;
            }

            if (artist.Tracks != null)
            {
                _context.Track.RemoveRange(artist.Tracks);                
            }

            if (artist.Albums != null)
            {
                _context.Album.RemoveRange(artist.Albums);
            }

            _context.Artists.Remove(artist);

            var user = await _userManager.FindByIdAsync(artist.UserId);

            if (user != null)
            {
                var res = await _userManager.RemoveFromRoleAsync(user, "artist");
                if (!res.Succeeded)
                {
                    return false;
                }
            }


            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteTrack(string userId, int trackId)
        {
            var track = await _context.Track.Include(b => b.Artist).FirstOrDefaultAsync(a => a.Id == trackId);

            if (track == null)
            {
                return false; 
            }

            if (track.Artist.UserId == userId)
            {
                _context.Track.Remove(track);
                await _context.SaveChangesAsync(); 
                return true; 
            }

            return false; 
        }

        public async Task<bool> DeleteAlbum(string userId, int albumId)
        {
            var album = await _context.Album
                .Include(b => b.Artist)
                .FirstOrDefaultAsync(a => a.Id == albumId);

            if (album == null)
            {
                return false;
            }

            if (album.Artist.UserId == userId)
            {
                var tracksToDelete = await _context.Track
                    .Where(t => t.AlbumId == albumId)
                    .ToListAsync();

                _context.Track.RemoveRange(tracksToDelete);
                _context.Album.Remove(album);

                await _context.SaveChangesAsync();

                return true;
            }

            return false;
        }



        public async Task<ActionResult<IEnumerable<Artist>>> GetAllArtists()
        {
            var artists = await _context.Artists.ToListAsync();
            return artists;
        }

        public async Task<int> GetArtistId(string userId)
        {
            var artist = await _context.Artists.FirstOrDefaultAsync(a => a.UserId == userId);
            return artist != null ? artist.Id : 0;
        }

        public async Task<bool> IsUserArtist(string userId)
        {
            var artist = await _context.Artists.FirstOrDefaultAsync(a => a.UserId == userId);

            return artist != null;
        }


        //do poprawy
        //sprawdz czy jest wlascicielem albumu
        //jak nie to wypierdol
        //najlepiej to przepisac calosc bo to jest na slowo honoru zrobione
        //ale zachlałem
        public async Task<bool> UpdateTrack(UpdateTrackVM model, string userId, string BlobUrl)
        {         
            var tack = await _context.Track.Include(a=>a.Artist).FirstOrDefaultAsync(a=>a.Id == model.trackId);
            if(tack == null)
            {
                return false;
            }

            if(tack.Artist.UserId != userId)
            {
                return false;
            }

            if (model.AlbumId == 0)
            {
                tack.Name = model.Name;
                tack.Img = model.Img;
                tack.BlobUrl= BlobUrl;
            }
            else
            {
                tack.Name = model.Name;
                tack.Img = model.Img;
                tack.AlbumId = model.AlbumId;
                tack.BlobUrl = BlobUrl;
            }

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> AddTrackToAlbum(int trackId, int albumId, int artistId)
        {
            var track = await _context.Track.FirstOrDefaultAsync(a => a.Id == trackId);

            if (track == null || track.ArtistId != artistId)
            {
                return false;
            }

            var album = await _context.Album.FirstOrDefaultAsync(a => a.Id == albumId);

            if (album == null || album.ArtistId != artistId)
            {
                return false;
            }

            track.AlbumId = albumId;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemoveFromAlbum(int trackId, int albumId, int artistId)
        {
            var track = await _context.Track.FirstOrDefaultAsync(a => a.Id == trackId);

            if (track == null || track.ArtistId != artistId)
            {
                return false;
            }

            var album = await _context.Album.FirstOrDefaultAsync(a => a.Id == albumId);

            if (album == null || album.ArtistId != artistId)
            {
                return false;
            }

            track.AlbumId = null;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ApplyForStudio(int artistId, int studioId)
        {
            var studio = await _context.Studio.FirstOrDefaultAsync(a=> a.Id==studioId);
            var artist = await _context.Artists.FirstOrDefaultAsync(a => a.Id == artistId);

            if (studio == null || artist == null)
            {
                return false;
            }
           
            if(artist.StudioId.HasValue)
            {
                return false;
            }

            var application = new ArtistRequest
            {
                StudioId = studioId,
                ArtistId = artistId,
                Date = DateTime.Now
            };

            _context.ArtistRequests.Add(application);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> AlreadyMadeRequest(int artistId, int studioId)
        {
            var existingRequest = await _context.ArtistRequests.FirstOrDefaultAsync(r => r.ArtistId == artistId && r.StudioId == studioId);

            if (existingRequest == null)
            {
                return false; 
            }
            return true;
        }

        public async Task<bool> DoesArtistHaveAStudio(int artistId)
        {
            var res = await _context.Artists.FirstOrDefaultAsync(a=>a.Id == artistId);
            if (res.StudioId.HasValue)
            {
                return true;
            }
            return false;
        }

        public async Task<string> GetArtistIdById(int artistId)
        {
            var res = await _context.Artists.FirstOrDefaultAsync(a => a.Id == artistId);
            return res.UserId;
        }

        public async Task<int> GetArtistIdByAlbumId(int albumId)
        {
            var res = await _context.Album.FirstOrDefaultAsync(a=> a.Id == albumId);
            if(res != null)
            {
                if(res.ArtistId != null) { return res.ArtistId.Value; }
                return 0;
            }
            return 0;
        }

        public async Task<int> GetArtistIdByTrackId(int trackId)
        {
            var res = await _context.Track.FirstOrDefaultAsync(a => a.Id == trackId);
            if (res != null)
            {
                if (res.ArtistId != null) { return res.ArtistId.Value; }
                return 0;
            }
            return 0;
        }

        public async Task<int> GetArtistIdByPaidAlbum(int albumId)
        {
            var res = await _context.PaidAlbums.FirstOrDefaultAsync(a => a.Id == albumId);
            if (res != null)
            {
                if (res.ArtistId != null) { return res.ArtistId.Value; }
                return 0;
            }
            return 0;
        }

        public async Task<int> GetArtistIdByPaidTrackId(int trackId)
        {
            var res = await _context.PaidTracks.FirstOrDefaultAsync(a => a.Id == trackId);
            if (res != null)
            {
                if (res.ArtistId != null) { return res.ArtistId.Value; }
                return 0;
            }
            return 0;
        }

        public async Task<int> GetArtistIdByPaidAlbumTrackId(int trackId)
        {
            var res = await _context.PaidAlbumTracks.FirstOrDefaultAsync(a => a.Id == trackId);
            if (res != null)
            {
                if (res.ArtistId != null) { return res.ArtistId.Value; }
                return 0;
            }
            return 0;
        }

        public async Task<bool> CreateAlbumWithTracks(CreateAlbumWithTracksVM model, int artistId)
        {
            var album = new Album
            {
                Name = model.Name,
                ArtistId = artistId,
                Desc = model.Desc,
                Img = model.Img,
            };

            _context.Album.Add(album);
            bool hasInvalidTracks = false;

            foreach (var track in model.Tracks)
            {
                string blobUrl = await _blobService.UploadBlob(track.Mp3File);

                if (string.IsNullOrEmpty(blobUrl))
                {
                    hasInvalidTracks = true;
                }

                var x = new Track
                {
                    Name = track.Name,
                    Img = track.Img,
                    BlobUrl = blobUrl,
                    ArtistId = artistId,
                    Album= album,
                };

                _context.Track.Add(x);
            }

            await _context.SaveChangesAsync();
            if (hasInvalidTracks == true) { return false; }
            return true;
        }

        public async Task<List<Album>> GetAlbumByUserId(string userId)
        {
            return await _context.Album.Where(a => a.Artist.UserId == userId).ToListAsync();
        }

    }
}
