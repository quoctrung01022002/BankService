using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Scaffolding.Internal;

namespace Demo.Entities;

public partial class TrungTq50demoContext : DbContext
{
    public TrungTq50demoContext()
    {
    }

    public TrungTq50demoContext(DbContextOptions<TrungTq50demoContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Refreshtoken> Refreshtokens { get; set; }

    public virtual DbSet<SellBuyRequest> SellBuyRequests { get; set; }

    public virtual DbSet<Stock> Stocks { get; set; }

    public virtual DbSet<Transaction> Transactions { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_0900_ai_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<Refreshtoken>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("refreshtoken");

            entity.HasIndex(e => e.UserId, "user_id");

            entity.Property(e => e.ExpireAt).HasColumnType("datetime");
            entity.Property(e => e.IssueAt).HasColumnType("datetime");
            entity.Property(e => e.JwtId).HasMaxLength(255);
            entity.Property(e => e.Token).HasColumnType("text");
            entity.Property(e => e.UserId)
                .HasMaxLength(10)
                .HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.Refreshtokens)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("refreshtoken_ibfk_1");
        });

        modelBuilder.Entity<SellBuyRequest>(entity =>
        {
            entity.HasKey(e => e.RequestId).HasName("PRIMARY");

            entity.ToTable("sell_buy_requests");

            entity.HasIndex(e => e.StockId, "stock_id");

            entity.HasIndex(e => e.UserId, "user_id");

            entity.Property(e => e.RequestId).HasColumnName("request_id");
            entity.Property(e => e.Price)
                .HasPrecision(10, 2)
                .HasColumnName("price");
            entity.Property(e => e.Quantity).HasColumnName("quantity");
            entity.Property(e => e.RequestDate)
                .HasColumnType("datetime")
                .HasColumnName("request_date");
            entity.Property(e => e.RequestType)
                .HasColumnType("enum('buy','sell')")
                .HasColumnName("request_type");
            entity.Property(e => e.StockId)
                .HasMaxLength(10)
                .HasColumnName("stock_id");
            entity.Property(e => e.UserId)
                .HasMaxLength(10)
                .HasColumnName("user_id");

            entity.HasOne(d => d.Stock).WithMany(p => p.SellBuyRequests)
                .HasForeignKey(d => d.StockId)
                .HasConstraintName("sell_buy_requests_ibfk_2");

            entity.HasOne(d => d.User).WithMany(p => p.SellBuyRequests)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("sell_buy_requests_ibfk_1");
        });

        modelBuilder.Entity<Stock>(entity =>
        {
            entity.HasKey(e => e.StockId).HasName("PRIMARY");

            entity.ToTable("stocks");

            entity.Property(e => e.StockId)
                .HasMaxLength(10)
                .HasColumnName("stock_id");
            entity.Property(e => e.CompanyName)
                .HasMaxLength(255)
                .HasColumnName("company_name");
            entity.Property(e => e.CurrentPrice)
                .HasPrecision(10, 2)
                .HasColumnName("current_price");
            entity.Property(e => e.UpdateDate).HasColumnName("update_date");
            entity.Property(e => e.Volume).HasColumnName("volume");
        });

        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(e => e.TransactionId).HasName("PRIMARY");

            entity.ToTable("transactions");

            entity.HasIndex(e => e.ReceiverId, "receiver_id");

            entity.HasIndex(e => e.SenderId, "sender_id");

            entity.HasIndex(e => e.StockId, "stock_id");

            entity.Property(e => e.TransactionId).HasColumnName("transaction_id");
            entity.Property(e => e.CurrentPrice)
                .HasPrecision(10, 2)
                .HasColumnName("current_price");
            entity.Property(e => e.Quantity).HasColumnName("quantity");
            entity.Property(e => e.ReceiverId)
                .HasMaxLength(10)
                .HasColumnName("receiver_id");
            entity.Property(e => e.SenderId)
                .HasMaxLength(10)
                .HasColumnName("sender_id");
            entity.Property(e => e.StockId)
                .HasMaxLength(10)
                .HasColumnName("stock_id");
            entity.Property(e => e.TransactionDate)
                .HasColumnType("datetime")
                .HasColumnName("transaction_date");
            entity.Property(e => e.TransactionType)
                .HasColumnType("enum('buy','sell')")
                .HasColumnName("transaction_type");

            entity.HasOne(d => d.Receiver).WithMany(p => p.TransactionReceivers)
                .HasForeignKey(d => d.ReceiverId)
                .HasConstraintName("transactions_ibfk_2");

            entity.HasOne(d => d.Sender).WithMany(p => p.TransactionSenders)
                .HasForeignKey(d => d.SenderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("transactions_ibfk_1");

            entity.HasOne(d => d.Stock).WithMany(p => p.Transactions)
                .HasForeignKey(d => d.StockId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("transactions_ibfk_3");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PRIMARY");

            entity.ToTable("users");

            entity.HasIndex(e => e.Phone, "phone").IsUnique();

            entity.HasIndex(e => e.Username, "username").IsUnique();

            entity.Property(e => e.UserId)
                .HasMaxLength(10)
                .HasColumnName("user_id");
            entity.Property(e => e.AccountBalance)
                .HasPrecision(10, 2)
                .HasColumnName("account_balance");
            entity.Property(e => e.Password)
                .HasMaxLength(100)
                .HasColumnName("password");
            entity.Property(e => e.Phone)
                .HasMaxLength(15)
                .HasColumnName("phone");
            entity.Property(e => e.Role)
                .HasDefaultValueSql("'User'")
                .HasColumnType("enum('Admin','User')")
                .HasColumnName("role");
            entity.Property(e => e.Username)
                .HasMaxLength(50)
                .HasColumnName("username");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
