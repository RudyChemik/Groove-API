using Groove.Models;

namespace Groove.VMO.Track
{
    public class GetAllTracksVM
    {
        public int Id { get; set; }

        public string Name { get; set; }
        public string? Img { get; set; }
        public string? BlobUrl { get; set; }
        public int? ArtistId { get; set; }
        public int? AlbumId { get; set; }
        public int? StudioId { get; set; }

        public string ArtistName { get; set; }
        public string StudioName { get; set; }
        public string AlbumName { get; set; }

    }
}
