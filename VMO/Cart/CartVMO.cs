using Groove.Models.ShoppingModels;

namespace Groove.VMO.Cart
{
    public class CartVMO
    {
        public int Quantity { get; set; }
        public double Price { get; set; }
        public string? TrackName { get; set; }
        public string? AlbumName { get; set; }
        public string Type { get; set; }
        public int ItemId { get; set; }
    }
}
