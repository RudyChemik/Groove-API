using Groove.Models;
using System.ComponentModel.DataAnnotations;

namespace Groove.VM.Artist
{
    public class ReqTrackVM
    {
        public string Name { get; set; }
        public string? Img { get; set; }
        public IFormFile Mp3File { get; set; }

    }
}
