using Groove.Models;
using Groove.VM;
using Groove.VM.Studio;
using Groove.VMO;

namespace Groove.Interfaces
{
    public interface IStudioService
    {
        Task<bool> CreateStudio(CreateStudioVM model, string userId);
        Task<bool> DoesUserHaveStudio(string userId);
        Task<List<Studio>> GetAllStudios();
        Task<int> GetStudioId(string userId);
        Task<List<StudioRequestsVM>> GetAllForStudio(int studioId);
        Task<bool> AcceptRequest(int requestId, string userId);
        Task<bool> DeclineRequest(int requestId, string userId);
        Task<bool> DeleteRequest(int requestId);
        Task<bool> IsArtistInStudio(int studioId, int artistId);
        Task<bool> MakeArtistAnAdmin(int studioId, string artistId);
        Task<bool> UpdateJWT(string artistId);
        Task<int> IsUserAnAdmin(string userId);
        Task<bool> CreateAlbumWithTracksFree(CreateFreeAlbumVM model, int studioId);
        Task<bool> CreateSingleFree(CreateSingleFreeVM model, int studioId, string blobURL);
        Task<bool> CreatePaidAlbumWithTracks(CreatePaidAlbumVM model, int studioId);
        Task<bool> CreatePaidSingle(CreateSinglePaidVM model, int studioId,string blobURL);
        Task<bool> DeleteAlbumFree(int albumId,int studioId);
        Task<bool> DeleteSingleFree(int albumId,int studioId);
        Task<bool> DeleteAlbumPaid(int albumId,int studioId);
        Task<bool> DeleteSinglePaid(int trackId, int studioId);
        Task<bool> RemoveFromFreeAlbum(int trackId, int albumId);
        Task<bool> RemoveFromPaidAlbum(int trackId, int albumId);
        Task<List<StudioAdmin>> GetAllAdmins(int studioId);
        Task<int> IsUserAnOwner(string userId);
        Task<bool> isUserStudioAdmin(string adminId, int studioId);
        Task<bool> RemoveFromAdmin(string adminId, int studioId);
        Task<bool> DeleteJWT(string adminId);
        Task<bool> IsAlbumBought(int albumId);
        Task<bool> IsArtistInStudioByUserId(int studioId, string userId);
        Task<bool> BecomeHead(int studioId, string userId);
        Task<bool> DeleteOldHead(string userId);
        Task<bool> DeleteArtistFromStudio(string artistId, int studioId);       
        Task<bool> RemoveHead(string userId);
        Task<bool> RemoveAdminRange(int studioId);
        Task<bool> DeleteStudio(int studioId);
        Task<List<Album>> GetAlbumByUserId(string userId);
        Task<List<Artist>> GetArtistByStudio(string userId);
    }
}
