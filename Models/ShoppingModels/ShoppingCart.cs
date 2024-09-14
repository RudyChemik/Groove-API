using Groove.Models.RequestModels;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Groove.Models.ShoppingModels
{
    public class ShoppingCart
    {
        
            [Key]
            public int Id { get; set; }

            public string UserId { get; set; }
            public virtual AppUser User { get; set; }
            public virtual List<ShoppingCartItem> Items { get; set; } 
        
    }
}

