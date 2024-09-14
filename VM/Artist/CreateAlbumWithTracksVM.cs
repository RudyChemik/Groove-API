using Groove.Models;
using System.ComponentModel.DataAnnotations;

namespace Groove.VM.Artist
{
    public class CreateAlbumWithTracksVM
    {
        public string Name { get; set; }
        public string Desc { get; set; }
        public string Img { get; set; }
        public List<ReqTrackVM> Tracks { get; set; }
    }
}
