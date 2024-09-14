namespace Groove.Models
{
    public class ArtistRequest
    {
        public int Id { get; set; }
        public int StudioId { get; set; }
        public Studio Studio { get; set; }
        public int ArtistId { get; set; }
        public Artist Artist { get; set; }       
        public DateTime Date { get; set; }
    }

}
