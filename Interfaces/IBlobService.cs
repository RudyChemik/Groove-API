namespace Groove.Interfaces
{
    public interface IBlobService
    {
        Task<string> UploadBlob(IFormFile file);
        Task<string> UploadBlobPaid(IFormFile file);
        Task<string> GetBlobTrack(int trackId);
        Task<bool> DeleteBlob(string blobUrl);
        Task<string> UploadBlobMainPic(IFormFile file);
    }
}
