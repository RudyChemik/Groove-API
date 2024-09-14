using Groove.Models;
using Groove.Models.RequestModels;

namespace Groove.VM
{
    public class CreateFreeAlbumVM
    {
        public string Name { get; set; }
        public string Desc { get; set; }
        public string Img { get; set; }
        public List<FreeTrack> Tracks { get; set; }
        public int ArtistId { get; set; }
    }
}
