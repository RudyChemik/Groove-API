using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Groove.Models.ShoppingModels
{
    public class ShoppingCartItem
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("User")]
        public string UserId { get; set; } 

        [ForeignKey("ShoppingCart")]
        public int ShoppingCartId { get; set; }

        public int ItemId { get; set; }
        public ItemType ItemType { get; set; }
        public int Quantity { get; set; }
        public double Price { get; set; }

        public virtual AppUser User { get; set; }
        public virtual ShoppingCart ShoppingCart { get; set; }
    }

    public enum ItemType
    {
        Album,
        Track
    }
}

