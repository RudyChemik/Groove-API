using Groove.Models;
using Groove.Models.App;
using Groove.Models.Order;
using Groove.Models.ShoppingModels;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Groove.Data
{
    public class AppDbContext : IdentityDbContext<AppUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Album>()
                .HasMany(a => a.Tracks)
                .WithOne(t => t.Album)
                .HasForeignKey(t => t.AlbumId);


            modelBuilder.Entity<Artist>()
               .HasMany(a => a.Tracks)
               .WithOne(t => t.Artist)
               .HasForeignKey(t => t.ArtistId);

            modelBuilder.Entity<Studio>()
                .HasMany(s => s.Albums)
                .WithOne(a => a.Studio)
                .HasForeignKey(a => a.StudioId);

            modelBuilder.Entity<UserLike>()
                .HasKey(ul => new { ul.AppUserId, ul.TrackId });

            modelBuilder.Entity<UserLike>()
                .HasOne(ul => ul.AppUser)
                .WithMany(u => u.LikedTracks)
                .HasForeignKey(ul => ul.AppUserId);

            modelBuilder.Entity<UserLike>()
                .HasOne(ul => ul.Track)
                .WithMany(t => t.LikedByUsers)
                .HasForeignKey(ul => ul.TrackId);

            modelBuilder.Entity<AlbumLike>()
               .HasKey(al => new { al.AppUserId, al.AlbumId });


            modelBuilder.Entity<AlbumLike>()
                .HasOne(al => al.AppUser)
                .WithMany(u => u.LikedAlbums)
                .HasForeignKey(al => al.AppUserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<AlbumLike>()
                .HasOne(al => al.Album)
                .WithMany(a => a.LikedByUsers)
                .HasForeignKey(al => al.AlbumId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Artist>()
            .HasOne(a => a.Studio)
            .WithMany(s => s.Artists)
            .HasForeignKey(a => a.StudioId);

            modelBuilder.Entity<Album>()
               .HasOne(a => a.Studio)
               .WithMany(s => s.Albums)
               .HasForeignKey(a => a.StudioId);

            modelBuilder.Entity<AppUser>()
            .HasOne(u => u.UserInformation)
            .WithOne(ui => ui.User)
            .HasForeignKey<UserInformation>(ui => ui.UserId);

            modelBuilder.Entity<ArtistRequest>()
            .HasOne(ar => ar.Studio)
            .WithMany(s => s.ArtistRequests)
            .HasForeignKey(ar => ar.StudioId);

            modelBuilder.Entity<ArtistRequest>()
                .HasOne(ar => ar.Artist)
                .WithMany(a => a.ArtistRequests)
                .HasForeignKey(ar => ar.ArtistId);

            modelBuilder.Entity<AppUser>()
            .HasMany(u => u.Orders)
            .WithOne(o => o.User)
            .HasForeignKey(o => o.UserId);

            modelBuilder.Entity<OrderHistory>()
                .HasOne(o => o.User)
                .WithMany(u => u.Orders)
                .HasForeignKey(o => o.UserId);

            modelBuilder.Entity<ShoppingCart>()
                .HasOne(cart => cart.User)
                .WithOne(user => user.ShoppingCart)
                .HasForeignKey<ShoppingCart>(cart => cart.UserId)
                .OnDelete(DeleteBehavior.Restrict); 

            modelBuilder.Entity<ShoppingCartItem>()
                .HasOne(item => item.User)
                .WithMany(user => user.ShoppingCartItems)
                .HasForeignKey(item => item.UserId)
                .OnDelete(DeleteBehavior.Restrict); 

            modelBuilder.Entity<ShoppingCartItem>()
                .HasOne(item => item.ShoppingCart)
                .WithMany(cart => cart.Items)
                .HasForeignKey(item => item.ShoppingCartId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<OrderedTrack>()
            .HasOne(orderItem => orderItem.OrderHistory)
            .WithMany(orderHistory => orderHistory.PurchasedItems)
            .HasForeignKey(orderItem => orderItem.OrderHistoryId);

            modelBuilder.Entity<OrderedTrack>()
                .HasOne(orderItem => orderItem.PaidTrack)
                .WithMany()
                .HasForeignKey(orderItem => orderItem.PaidTrackId);

            modelBuilder.Entity<OrderedAlbums>()
           .HasOne(orderItem => orderItem.OrderHistory)
           .WithMany(orderHistory => orderHistory.PurchasedAlbums)
           .HasForeignKey(orderItem => orderItem.OrderHistoryId);

            modelBuilder.Entity<OrderedAlbums>()
                .HasOne(orderItem => orderItem.PaidAlbum)
                .WithMany()
                .HasForeignKey(orderItem => orderItem.PaidAlbumId);

            base.OnModelCreating(modelBuilder);
        }

        public DbSet<AppUser> AppUser { get; set; }
        public DbSet<Track> Track { get; set; }
        public DbSet<Album> Album { get; set; }
        public DbSet<Studio> Studio { get; set; }
        public DbSet<UserLike> UserLikes { get; set; } 
        public DbSet<AlbumLike> AlbumLikes { get; set; }
        public DbSet<UserInformation> UserInformation { get; set; }
        public DbSet<Artist> Artists { get; set; }  
        public DbSet<PaidTracks> PaidTracks { get; set; }
        public DbSet<PaidAlbums> PaidAlbums { get; set; }
        public DbSet<PaidAlbumTrack> PaidAlbumTracks { get; set; }
        public DbSet<StudioAdmin> StudioAdmins { get; set; }
        public DbSet<ArtistRequest> ArtistRequests { get; set; }
        public DbSet<OrderHistory> OrderHistory { get; set; }
        public DbSet<ShoppingCart> ShoppingCarts { get; set; }
        public DbSet<ShoppingCartItem> ShoppingCartItems { get; set; }
        public DbSet<OrderedTrack> OrderedTrack { get; set; }
        public DbSet<OrderedAlbums> OrderedAlbums { get; set; }
        public DbSet<AppStatus> AppStatus { get; set; }
        public DbSet<AppRes> AppRes { get; set; }
    }
}
