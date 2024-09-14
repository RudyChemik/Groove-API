using System;
using System.Collections.Generic;
using System.Linq;
using Groove.Models;
using Groove.Models.App;
using Groove.Models.Order;
using Groove.Models.ShoppingModels;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Groove.Data
{
    public static class AppDbInitializer
    {
        public static void Initialize(IApplicationBuilder applicationBuilder)
        {
            using (var serviceScope = applicationBuilder.ApplicationServices.CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetService<AppDbContext>();

                context.Database.EnsureCreated();
                SeedArtists(context);
                //SeedAlbums(context);
                //SeedTracks(context);
                //SeedStudios(context);
                //SeedPaidAlbums(context);
                //SeedPaidAlbumTracks(context);
                //SeedPaidTracks(context);
                //SeedAppStatus(context);
            }
        }


        private static void SeedArtists(AppDbContext context)
        {
            if (!context.Artists.Any())
            {
                context.Artists.AddRange(new List<Artist>
                {
                    new Artist
                    {
                        Name = "chuj",
                        Desc = "aaa",
                        Img = "gigaspo",
                        UserId = "afgasfas",
                    },
                });
                context.SaveChanges();
            }
        }

        //private static void SeedAlbums(AppDbContext context)
        //{
        //    if (!context.Album.Any())
        //    {
        //        context.Album.AddRange(new List<Album>
        //        {
        //            new Album
        //            {
        //            },
        //        });
        //        context.SaveChanges();
        //    }
        //}

        //private static void SeedTracks(AppDbContext context)
        //{
        //    if (!context.Track.Any())
        //    {
        //        context.Track.AddRange(new List<Track>
        //        {
        //            new Track
        //            {
                        
        //            },
        //        });
        //        context.SaveChanges();
        //    }
        //}

        //private static void SeedStudios(AppDbContext context)
        //{
        //    if (!context.Studio.Any())
        //    {
        //        context.Studio.AddRange(new List<Studio>
        //        {
        //            new Studio
        //            {

        //            },

        //        });
        //        context.SaveChanges();
        //    }
        //}

        //private static void SeedPaidTracks(AppDbContext context)
        //{

        //}

        //private static void SeedPaidAlbums(AppDbContext context)
        //{

        //}

        //private static void SeedPaidAlbumTracks(AppDbContext context)
        //{

        //}

        //private static void SeedAppStatus(AppDbContext context)
        //{

        //}

    }
}
