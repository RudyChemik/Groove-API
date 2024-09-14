using System.ComponentModel.DataAnnotations;

namespace Groove.Models
{
    public class Album
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }
        public string Desc { get; set; }
        public string Img { get; set; }

        public List<Track>? Tracks { get; set; }

        public int? ArtistId { get; set; }
        public Artist? Artist { get; set; }

        public int? StudioId { get; set; }
        public Studio? Studio { get; set; }
        public List<AlbumLike> LikedByUsers { get; set; }
    }
}
