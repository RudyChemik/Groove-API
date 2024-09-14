using Groove.Models;
using Groove.Models.RequestModels;

namespace Groove.VM.Studio
{
    public class CreatePaidAlbumVM
    {
        public string Name { get; set; }
        public string Desc { get; set; }
        public string Img { get; set; }
        public double Price { get; set; }
        public List<PaidTrack> Tracks { get; set; }
        public int ArtistId { get; set; }
    }
}
