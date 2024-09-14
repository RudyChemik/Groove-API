using System.ComponentModel.DataAnnotations;

namespace Groove.Models
{
    public class AlbumLike
    {
        public int AlbumId { get; set; }
        public Album Album { get; set; }

        public string AppUserId { get; set; }
        public AppUser AppUser { get; set; }
    }

}
