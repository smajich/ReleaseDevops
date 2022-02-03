﻿using System.Data.Entity;

namespace MvcMusicStore.Models
{
    public interface IMusicStoreEntities
    {
        DbSet<Album> Albums { get; set; }
        DbSet<Artist> Artists { get; set; }
        DbSet<Cart> Carts { get; set; }
        DbSet<Genre> Genres { get; set; }
        DbSet<OrderDetail> OrderDetails { get; set; }
        DbSet<Order> Orders { get; set; }
        //DbSet<Product> Products { get; set; }
        //DbSet<ProductCategory> Categories { get; set; }
    }
}