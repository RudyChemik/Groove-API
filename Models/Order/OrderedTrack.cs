using System.ComponentModel.DataAnnotations;

namespace Groove.Models.Order
{
    public class OrderedTrack
    {
        [Key]
        public int Id { get; set; }

        public int OrderHistoryId { get; set; }
        public OrderHistory OrderHistory { get; set; }

        public int? PaidTrackId { get; set; }
        public PaidTracks? PaidTrack { get; set; }
        public int? qty { get; set; }
    }

}
