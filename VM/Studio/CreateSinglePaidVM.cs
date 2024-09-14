namespace Groove.VM.Studio
{
    public class CreateSinglePaidVM
    {
        public string Name { get; set; }
        public string Img { get; set; }
        public double Price { get; set; }
        public int ArtistId { get; set; }
        public IFormFile Mp3File { get; set; }
    }
}
