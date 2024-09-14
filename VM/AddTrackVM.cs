namespace Groove.VM
{
    public class AddTrackVM
    {
        public string Name { get; set; }
        public string Img { get; set; }
        public int? AlbumId { get; set; }
        public IFormFile? Mp3File { get; set; }
    }
}
