namespace Groove.VM.Track
{
    public class UpdateTrackVM
    {
        public int trackId { get; set; }
        public string Name { get; set; }
        public string Img { get; set; }
        public int? AlbumId { get; set; }
        public IFormFile Mp3File { get; set; }
    }
}
