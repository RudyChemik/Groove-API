namespace Groove.Models
{
    public class UserLike
    {
        public string AppUserId { get; set; }
        public AppUser AppUser { get; set; }

        public int TrackId { get; set; }
        public Track Track { get; set; }
        
    }
}
