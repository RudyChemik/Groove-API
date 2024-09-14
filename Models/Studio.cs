namespace Groove.Models
{
    public class Studio
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Localization { get; set; }
        public string? Img { get; set; }
        public DateTime CreateDate { get; set; }
        public string? AdressUrl { get; set; }

        public List<Artist>? Artists { get; set; }
        public List<Album>? Albums { get; set; }
        public List<Track>? Tracks { get; set; }
        
        //PPW
        public List<PaidTracks>? PaidTracks { get; set; }
        public List<PaidAlbums>? PaidAlbums { get; set; }

        public string OwnerId { get; set; }
        public List<StudioAdmin>? Admins { get; set; }
        public List<ArtistRequest> ArtistRequests { get; set; }
    }
}
