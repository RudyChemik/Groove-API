using Groove.Migrations;
using System.ComponentModel.DataAnnotations;

namespace Groove.Models
{
    public class Artist
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }
        public string? Desc { get; set; }
        public string? Img { get; set; }

        public string UserId { get; set; }

        public List<Track>? Tracks { get; set; }
        public List<Album>? Albums { get; set; }

        public int? StudioId { get; set; }
        public Studio? Studio { get; set; }
        public List<ArtistRequest> ArtistRequests { get; set; }
    }
}
