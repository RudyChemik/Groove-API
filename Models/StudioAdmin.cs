namespace Groove.Models
{
    public class StudioAdmin
    {
        public int Id { get; set; }

        public int StudioId { get; set; }
        public Studio Studio { get; set; }


        public string UserId { get; set; }
        public AppUser User { get; set; }
    }
}
