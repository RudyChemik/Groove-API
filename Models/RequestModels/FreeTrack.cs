namespace Groove.Models.RequestModels
{
    public class FreeTrack
    {
        public string Name { get; set; }
        public string? Img { get; set; }
        public IFormFile Mp3File { get; set; }
    }
}
