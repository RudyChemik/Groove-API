using Groove.Models.ShoppingModels;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace Groove.Models
{
    public class AppUser:IdentityUser
    {
        public string? Name { get; set; }        

        public double Balance { get; set; } = 1000;

        public List<UserLike> LikedTracks { get; set; }

        public List<AlbumLike> LikedAlbums { get; set; }
        public UserInformation UserInformation { get; set; }

        public List<OrderHistory> Orders { get; set; }

        public List<ShoppingCartItem> ShoppingCartItems { get; set; }
        public virtual ShoppingCart ShoppingCart { get; set; }
    }
}
