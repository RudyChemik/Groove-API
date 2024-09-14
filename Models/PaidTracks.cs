using System.ComponentModel.DataAnnotations;

namespace Groove.Models
{
    public class PaidTracks
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }
        public string? Img { get; set; }
        public double Price { get; set; }
        public int? ArtistId { get; set; }
        public Artist? Artist { get; set; }

        public string? BlobUrl { get; set; }

        public int StudioId { get; set; }
        public Studio Studio { get; set; }

        public bool IsVisible { get; set; } = true;

    }
}
