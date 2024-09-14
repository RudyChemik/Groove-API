using System.ComponentModel.DataAnnotations;

namespace Groove.Models.Order
{
    public class OrderedAlbums
    {
        [Key]
        public int Id { get; set; }

        public int OrderHistoryId { get; set; }
        public OrderHistory OrderHistory { get; set; }

        public int? PaidAlbumId { get; set; }
        public PaidAlbums? PaidAlbum { get; set; }
        public int? qty { get; set; }
    }
}
