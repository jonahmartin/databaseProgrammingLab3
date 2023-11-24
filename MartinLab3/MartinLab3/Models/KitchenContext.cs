﻿using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Microsoft.EntityFrameworkCore;

namespace MartinLab3.Models;
/*Scaffold-DbContext -Connection "Data Source=(local)\SQLEXPRESS;Initial Catalog=KitchenSupply;
Integrated Security=True;TrustServerCertificate=True" 
-Provider Microsoft.EntityFrameworkCore.SqlServer -OutputDir Models -Context KitchenContext -DataAnnotations -Force*/
public partial class KitchenContext : DbContext
{
    public KitchenContext()
    {
    }

    public KitchenContext(DbContextOptions<KitchenContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Customer> Customers { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<OrderItem> OrderItems { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<Service> Services { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Data Source=(local)\\SQLEXPRESS;Initial Catalog=KitchenSupply;Integrated Security=True;TrustServerCertificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.CustomerId).HasName("PK__Customer__A4AE64B8515BD35A");
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.OrderId).HasName("PK__Orders__C3905BAF9F12CCDA");

            entity.HasOne(d => d.Customer).WithMany(p => p.Orders).HasConstraintName("FK_Orders_Customers");
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasKey(e => e.LineId).HasName("PK__OrderIte__2EAE64C986F2538B");

            entity.HasOne(d => d.Order).WithMany(p => p.OrderItems).HasConstraintName("FK_OrderItems_Orders");

            entity.HasOne(d => d.Product).WithMany(p => p.OrderItems).HasConstraintName("FK_OrderItems_Products");

            entity.HasOne(d => d.Service).WithMany(p => p.OrderItems).HasConstraintName("FK_OrderItems_Services");
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.ProductId).HasName("PK__Products__B40CC6EDED1A39E6");
        });

        modelBuilder.Entity<Service>(entity =>
        {
            entity.HasKey(e => e.ServiceId).HasName("PK__Services__C51BB0EA67D53234");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
