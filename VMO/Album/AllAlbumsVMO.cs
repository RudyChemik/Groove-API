using Groove.Models;

namespace Groove.VMO.Album
{
    public class AllAlbumsVMO
    {
        public int AlbumId { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? AlbumName { get; set; }
        public string? StudioName { get; set; }
        public List<Models.Track>? Tracks { get; set; }
    }

}
