namespace Groove.Models
{
    public class PaidAlbumTrack
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Img { get; set; }

        public string? BlobUrl { get; set; }
        public int? ArtistId { get; set; }
        public Artist? Artist { get; set; }
        public int? StudioId { get; set; }
        public Studio Studio { get; set; }
        public int PaidAlbumId { get; set; }
        public PaidAlbums? PaidAlbum { get; set; }
    }
}
