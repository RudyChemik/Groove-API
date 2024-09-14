using Groove.Models.Order;
using System.ComponentModel.DataAnnotations;

namespace Groove.Models
{
    public class OrderHistory
    {
        [Key]
        public int Id { get; set; }
        public string UserId { get; set; }
        public AppUser User { get; set; }

        public string OrderNumber { get; set; }
        public double OrderCost { get; set; }
        public bool isPies { get; set; }
        public DateTime OrderDate { get; set; }
        public List<OrderedTrack> PurchasedItems { get; set; }
        public List<OrderedAlbums> PurchasedAlbums { get; set; }
    }
}
