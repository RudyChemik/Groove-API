using Azure.Storage.Blobs;
using Groove.Data;
using Groove.Interfaces;
using Groove.Migrations;
using Groove.Models;
using Groove.VM;
using Groove.VM.Studio;
using Groove.VMO;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Web.Http.Tracing;
using System.Xml.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Groove.Services
{
    public class StudioService : IStudioService
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly string _connectionString;
        public StudioService(IConfiguration configuration, AppDbContext appDbContext, UserManager<AppUser> userManager)
        {
            _userManager = userManager;
            _connectionString = configuration["ConnectionStrings:AzureBlobStorage"];
            _context = appDbContext;
        }

        public async Task<bool> AcceptRequest(int requestId, string userId)
        {
            var res = await _context.ArtistRequests.Include(s => s.Studio).FirstOrDefaultAsync(a => a.Id == requestId);

            if (res == null)
            {
                return false;
            }

            if (res.Studio.OwnerId != userId)
            {
                return false;
            }

            var artist = await _context.Artists.FirstOrDefaultAsync(a => a.Id == res.ArtistId);
            if (artist == null)
            {
                return false;
            }

            artist.StudioId = res.StudioId;
            await _context.SaveChangesAsync();

            return true;

        }

        public async Task<bool> CreateStudio(CreateStudioVM model, string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return false;
            }

            await _userManager.AddToRoleAsync(user, "studiohead");

            var studio = new Studio
            {
                OwnerId = userId,
                Name = model.Name,
                Img = model.Img,
                Localization = model.Localization,
                CreateDate = DateTime.Now,
                AdressUrl = model.AdressUrl,
            };

            _context.Studio.Add(studio);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeclineRequest(int requestId, string userId)
        {
            var res = await _context.ArtistRequests.Include(s => s.Studio).FirstOrDefaultAsync(a => a.Id == requestId);

            if (res == null)
            {
                return false;
            }

            if (res.Studio.OwnerId != userId)
            {
                return false;
            }

            return true;
        }

        public async Task<bool> DeleteRequest(int requestId)
        {
            var res = await _context.ArtistRequests.FirstOrDefaultAsync(a => a.Id == requestId);
            if (res != null)
            {
                _context.ArtistRequests.Remove(res);
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task<bool> DoesUserHaveStudio(string userId)
        {
            var isOwner = await _context.Studio.AnyAsync(a => a.OwnerId == userId);
            if (isOwner)
            {
                return true;
            }

            var isAdmin = await _context.StudioAdmins.AnyAsync(a => a.UserId == userId);
            if (isAdmin)
            {
                return true;
            }

            return false;

        }

        public async Task<List<StudioRequestsVM>> GetAllForStudio(int studioId)
        {
            var res = await _context.ArtistRequests
                .Where(a => a.StudioId == studioId)
                .Select(a => new StudioRequestsVM
                {
                    requestId = a.Id,
                    ArtistId = a.ArtistId,
                    ArtistName = a.Artist.Name,
                    Created = a.Date
                })
                .ToListAsync();

            return res;
        }



        public async Task<List<Studio>> GetAllStudios()
        {
            var res = await _context.Studio.ToListAsync();
            return res;
        }

        public async Task<int> GetStudioId(string userId)
        {
            var res = await _context.Studio.FirstOrDefaultAsync(a => a.OwnerId == userId);
            return res != null ? res.Id : 0;
        }

        public async Task<bool> IsArtistInStudio(int studioId, int artistId)
        {
            var res = await _context.Artists.FirstOrDefaultAsync(a => a.Id == artistId);

            if (res == null) { return false; }

            if (res.StudioId != studioId) return false;

            return true;
        }

        public async Task<bool> MakeArtistAnAdmin(int studioId, string artistId)
        {
            var already = await _context.StudioAdmins.FirstOrDefaultAsync(a => a.StudioId == studioId && a.UserId == artistId);

            if (already != null) { return false; }

            var newADM = new StudioAdmin
            {
                StudioId = studioId,
                UserId = artistId,
            };

            _context.StudioAdmins.Add(newADM);
            await _context.SaveChangesAsync();
            return true;

        }

        public async Task<bool> UpdateJWT(string artistId)
        {
            var user = await _userManager.FindByIdAsync(artistId);
            if (user != null)
            {
                await _userManager.AddToRoleAsync(user, "studioadmin");
            }
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteJWT(string adminId)
        {
            var user = await _userManager.FindByIdAsync(adminId);
            if (user != null)
            {
                var res = await _userManager.RemoveFromRoleAsync(user, "artist");
                if (!res.Succeeded)
                {
                    return false;
                }
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }


        public async Task<int> IsUserAnAdmin(string userId)
        {
            var res = await _context.StudioAdmins.FirstOrDefaultAsync(a => a.UserId == userId);
            if (res == null)
            {
                var head = await _context.Studio.FirstOrDefaultAsync(a => a.OwnerId == userId);
                if (head != null) { return head.Id; }

                return 0;
            }

            return res.StudioId;

        }

        public async Task<bool> CreateAlbumWithTracksFree(CreateFreeAlbumVM model, int studioId)
        {
            var album = new Album
            {
                Name = model.Name,
                ArtistId = model.ArtistId,
                StudioId = studioId,
                Desc = model.Desc,
                Img = model.Img,
            };

            _context.Album.Add(album);
            bool hasInvalidTracks = false;

            foreach (var track in model.Tracks)
            {
                string blobUrl = await UploadBlob(track.Mp3File);

                if (string.IsNullOrEmpty(blobUrl))
                {
                    hasInvalidTracks = true;
                }

                var x = new Track
                {
                    Name = track.Name,
                    Img = track.Img,
                    BlobUrl = blobUrl
                };

                x.Album = album;
                x.StudioId = studioId;
                x.ArtistId = model.ArtistId;

                _context.Track.Add(x);
            }

            await _context.SaveChangesAsync();
            if (hasInvalidTracks == true)
            {
                return false;
            }
            return true;
        }

        public async Task<bool> CreateSingleFree(CreateSingleFreeVM model, int studioId, string blobURL)
        {
            var track = new Track { Name = model.Name, ArtistId = model.ArtistId, StudioId = studioId, Img = model.Img, BlobUrl = blobURL };

            _context.Track.Add(track);
            await _context.SaveChangesAsync();

            return true;
        }
        //na slowo honoru
        //xd
        public async Task<bool> CreatePaidAlbumWithTracks(CreatePaidAlbumVM model, int studioId)
        {
            var album = new PaidAlbums
            {
                Name = model.Name,
                ArtistId = model.ArtistId,
                StudioId = studioId,
                Price = model.Price,
                Desc = model.Desc,
                Img = model.Img,
            };

            _context.PaidAlbums.Add(album);
            bool hasInvalidTracks = false;

            foreach (var track in model.Tracks)
            {
                string blobUrl = await UploadBlobPaid(track.Mp3File);

                if (string.IsNullOrEmpty(blobUrl))
                {
                    hasInvalidTracks = true;
                }

                var x = new PaidAlbumTrack
                {
                    Name = track.Name,
                    Img = track.Img,
                    BlobUrl = blobUrl,
                };

                x.PaidAlbum = album;
                x.StudioId = studioId;
                x.ArtistId = model.ArtistId;

                _context.PaidAlbumTracks.Add(x);
            }

            await _context.SaveChangesAsync();
            if (hasInvalidTracks == true) { return false; }
            return true;
        }

        public async Task<bool> CreatePaidSingle(CreateSinglePaidVM model, int studioId, string blobUrl)
        {
            var track = new PaidTracks { Name = model.Name, ArtistId = model.ArtistId, StudioId = studioId, Img = model.Img, Price = model.Price, BlobUrl = blobUrl };

            _context.PaidTracks.Add(track);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteAlbumFree(int albumId, int studioId)
        {
            var album = await _context.Album
                .FirstOrDefaultAsync(a => a.Id == albumId);

            if (album == null)
            {
                return false;
            }

            if (album.StudioId == studioId)
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

        public async Task<bool> DeleteSingleFree(int trackId, int studioId)
        {
            var track = await _context.Track
               .FirstOrDefaultAsync(a => a.Id == trackId);

            if (track == null)
            {
                return false;
            }

            if (track.StudioId == studioId)
            {
                _context.Track.Remove(track);

                await _context.SaveChangesAsync();

                return true;
            }

            return false;

        }

        public async Task<bool> DeleteAlbumPaid(int albumId, int studioId)
        {

            var album = await _context.PaidAlbums
                .FirstOrDefaultAsync(a => a.Id == albumId);

            if (album == null)
            {
                return false;
            }

            var anybought = await _context.OrderHistory.AnyAsync(order => order.PurchasedAlbums.Any(item => item.PaidAlbumId == albumId));

            if (album.StudioId == studioId)
            {
                if (anybought)
                {
                    album.IsVisible = false;
                    await _context.SaveChangesAsync();
                    return true;
                }

                var tracksToDelete = await _context.PaidAlbumTracks
                    .Where(t => t.PaidAlbumId == albumId)
                    .ToListAsync();

                _context.PaidAlbumTracks.RemoveRange(tracksToDelete);
                _context.PaidAlbums.Remove(album);

                await _context.SaveChangesAsync();

                return true;
            }

            return false;
        }

        public async Task<bool> DeleteSinglePaid(int trackId, int studioId)
        {
            var track = await _context.PaidTracks
               .FirstOrDefaultAsync(a => a.Id == trackId);

            if (track == null)
            {
                return false;
            }

            var anybought = await _context.OrderHistory.AnyAsync(order => order.PurchasedItems.Any(item => item.PaidTrackId == trackId));


            if (track.StudioId == studioId)
            {
                if (anybought)
                {
                    track.IsVisible = false;
                    await _context.SaveChangesAsync();
                    return true;
                }

                _context.PaidTracks.Remove(track);

                await _context.SaveChangesAsync();

                return true;
            }

            return false;

        }

        public async Task<bool> RemoveFromFreeAlbum(int trackId, int albumId)
        {
            var album = await _context.Album.FirstOrDefaultAsync(a => a.Id == albumId);
            if (album == null) { return false; }

            var track = await _context.Track.FirstOrDefaultAsync(a => a.Id == trackId); if (track == null) { return false; }

            if (track.AlbumId == albumId)
            {
                track.AlbumId = null;
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task<List<StudioAdmin>> GetAllAdmins(int studioId)
        {
            var res = await _context.StudioAdmins.Where(a => a.StudioId == studioId).ToListAsync();
            return res;
        }

        public async Task<bool> RemoveFromPaidAlbum(int trackId, int albumId)
        {
            var album = await _context.PaidAlbums.FirstOrDefaultAsync(a => a.Id == albumId);
            if (album == null) { return false; }

            var track = await _context.PaidAlbumTracks.FirstOrDefaultAsync(a => a.Id == trackId); if (track == null) { return false; }

            if (track.PaidAlbumId == albumId)
            {
                _context.Remove(track);
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task<bool> IsAlbumBought(int albumId)
        {
            var anybought = await _context.OrderHistory.AnyAsync(order => order.PurchasedAlbums.Any(item => item.PaidAlbumId == albumId));
            if (anybought) { return false; }

            return anybought;

        }

        public async Task<int> IsUserAnOwner(string userId)
        {
            var res = await _context.Studio.FirstOrDefaultAsync(a => a.OwnerId == userId);
            return res != null ? res.Id : 0;
        }

        public async Task<bool> isUserStudioAdmin(string adminId, int studioId)
        {
            var res = await _context.StudioAdmins.FirstOrDefaultAsync(a => a.UserId == adminId && a.StudioId == studioId);
            if (res != null) { return true; }
            return false;
        }

        public async Task<bool> RemoveFromAdmin(string adminId, int studioId)
        {
            var res = await _context.StudioAdmins.FirstOrDefaultAsync(a => a.UserId == adminId && a.StudioId == studioId);
            if (res != null)
            {
                _context.Remove(res);
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }

        private async Task<string> UploadBlob(IFormFile file)
        {
            var containerName = "tracks";
            var blobServiceClient = new BlobServiceClient(_connectionString);
            var blobContainer = blobServiceClient.GetBlobContainerClient(containerName);
            await blobContainer.CreateIfNotExistsAsync();

            var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            var blobClient = blobContainer.GetBlobClient(uniqueFileName);
            await blobClient.UploadAsync(file.OpenReadStream());

            return blobClient.Uri.ToString();
        }

        private async Task<string> UploadBlobPaid(IFormFile file)
        {
            var containerName = "paidtracks";
            var blobServiceClient = new BlobServiceClient(_connectionString);
            var blobContainer = blobServiceClient.GetBlobContainerClient(containerName);
            await blobContainer.CreateIfNotExistsAsync();

            var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            var blobClient = blobContainer.GetBlobClient(uniqueFileName);
            await blobClient.UploadAsync(file.OpenReadStream());

            return blobClient.Uri.ToString();
        }

        public async Task<bool> IsArtistInStudioByUserId(int studioId, string userId)
        {
            var studio = await _context.Studio.Include(a => a.Artists).FirstOrDefaultAsync(s => s.Id == studioId);

            if (studio == null)
            {
                return false;
            }

            var res = studio.Artists.Any(artist => artist.UserId == userId);
            if (res == true) { return true; }
            return false;
        }

        public async Task<bool> BecomeHead(int studioId, string userId)
        {
            var studio = await _context.Studio.FirstOrDefaultAsync(a => a.Id == studioId);
            if (studio == null) { return false; }

            studio.OwnerId = userId;
            await _context.SaveChangesAsync();

            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return false;
            }

            await _userManager.AddToRoleAsync(user, "studiohead");
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteOldHead(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return false;
            }
            var res = await _userManager.RemoveFromRoleAsync(user, "studiohead");
            if (!res.Succeeded)
            {
                return false;
            }
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemoveAdminRange(int studioId)
        {
            var admins = await _context.StudioAdmins.Where(sa => sa.StudioId == studioId).ToListAsync();

            foreach (var admin in admins)
            {
                var user = await _userManager.FindByIdAsync(admin.UserId);
                if (user != null)
                {
                    var result = await _userManager.RemoveFromRoleAsync(user, "studioadmin");
                    if (!result.Succeeded)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public async Task<bool> RemoveHead(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return false;
            }
            await _userManager.RemoveFromRoleAsync(user, "studiohead");
            return true;
        }

        public async Task<bool> DeleteStudio(int studioId)
        {
            var studio = await _context.Studio
               .Include(s => s.Artists)
               .Include(s => s.Albums)
               .Include(s => s.Tracks)
               .Include(s => s.PaidTracks)
               .Include(s => s.Admins)
               .FirstOrDefaultAsync(s => s.Id == studioId);

            if (studio == null)
            {
                return false;
            }

            studio.Artists?.ForEach(a => a.StudioId = null);
            studio.PaidTracks?.ForEach(pt => pt.IsVisible = false);
            studio.PaidAlbums?.ForEach(pt => pt.IsVisible = false);

            _context.Track.RemoveRange(studio.Tracks);
            _context.Album.RemoveRange(studio.Albums);

            _context.Studio.Remove(studio);
            await _context.SaveChangesAsync();
            return true;

        }

        public async Task<bool> DeleteArtistFromStudio(string artistId, int studioId)
        {
            var artist = await _context.Artists.FirstOrDefaultAsync(a => a.UserId == artistId);
            if (artist == null) { return false; }

            artist.StudioId = null;
            await _context.SaveChangesAsync();

            var studioadm = await _context.StudioAdmins.FirstOrDefaultAsync(a => a.StudioId == studioId && a.UserId == artistId);
            if (studioadm == null) { return true; }

            _context.Remove(studioadm);

            var user = await _userManager.FindByIdAsync(artistId);

            if (user == null)
            {
                return false;
            }
            var res = await _userManager.RemoveFromRoleAsync(user, "studioadmin");

            return true;
        }

        public async Task<List<Album>> GetAlbumByUserId(string userId)
        {
            return await _context.Album.Where(a => a.Studio.Artists.Any(artist => artist.UserId == userId)).ToListAsync();
        }

        public async Task<List<Artist>> GetArtistByStudio(string userId)
        {
            return await _context.Artists.Where(artist => artist.Studio.OwnerId == userId).ToListAsync();
        }
    }
}