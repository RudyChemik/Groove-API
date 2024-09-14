using Azure.Storage.Blobs;
using Groove.Data;
using Groove.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Groove.Services
{
    public class BlobService:IBlobService
    {
        private readonly string _connectionString;
        private readonly AppDbContext _context;

        public BlobService(IConfiguration configuration, AppDbContext context)
        {
            _connectionString = configuration["ConnectionStrings:AzureBlobStorage"];
            _context = context;
        }

        public async Task<string> UploadBlob(IFormFile file)
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

        public async Task<string> UploadBlobPaid(IFormFile file)
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

        public async Task<string> UploadBlobMainPic(IFormFile file)
        {
            var containerName = "main";
            var blobServiceClient = new BlobServiceClient(_connectionString);
            var blobContainer = blobServiceClient.GetBlobContainerClient(containerName);
            await blobContainer.CreateIfNotExistsAsync();

            var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            var blobClient = blobContainer.GetBlobClient(uniqueFileName);
            await blobClient.UploadAsync(file.OpenReadStream());

            return blobClient.Uri.ToString();
        }


        public async Task<bool> DeleteBlob(string blobUrl)
        {
            if (!string.IsNullOrEmpty(blobUrl))
            {
                var blobUri = new Uri(blobUrl);
                var blobServiceClient = new BlobServiceClient(_connectionString);

                var containerName = blobUri.Segments[1].TrimEnd('/');
                var blobName = string.Join("", blobUri.Segments.Skip(2));

                var blobContainerClient = blobServiceClient.GetBlobContainerClient(containerName);
                var blobClient = blobContainerClient.GetBlobClient(blobName);

                await blobClient.DeleteIfExistsAsync();
                return true;
            }
            return false;
        }


        public async Task<string> GetBlobTrack(int trackId)
        {
            var res = await _context.Track.FirstOrDefaultAsync(a => a.Id == trackId);
            return res.BlobUrl;
        }
    }
}
