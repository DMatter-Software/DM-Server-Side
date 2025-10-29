using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static DMServer.db.Subscription;

namespace DMServer.db
{
    public class Database : DbContext
    {
        public Database(DbContextOptions<Database> options)
            : base(options)
        {
        }

        public DbSet<User> Users => Set<User>();
        public DbSet<Subscription> Subscriptions => Set<Subscription>();
        public DbSet<Log> Logs => Set<Log>();
        public DbSet<Message> Messages => Set<Message>();
        public DbSet<Session> Sessions => Set<Session>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Подписка -> владелец
            modelBuilder.Entity<Subscription>()
                .HasOne(s => s.Account)
                .WithMany()
                .HasForeignKey(s => s.AccountId)
                .OnDelete(DeleteBehavior.Cascade);

            // Подписка -> реферал
            modelBuilder.Entity<Subscription>()
                .Property(s => s.Status)
                .HasConversion<string>()   // храним как строку
                .HasDefaultValue(Subscription.Status_Subscription.inactive)
                .HasColumnType("ENUM('active','paused','inactive','suspended')");

            modelBuilder.Entity<Log>()
                .HasOne<User>()
                .WithMany()
                .HasForeignKey(l => l.AuthorId);

            modelBuilder.Entity<Message>()
                .HasOne<User>()
                .WithMany()
                .HasForeignKey(m => m.AuthorId);

            modelBuilder.Entity<Session>()
                .HasOne<User>()
                .WithMany()
                .HasForeignKey(s => s.AccountId);
        }

    }

    public class User
    {
        [Key]
        public long AccountId { get; set; }
        public string Login { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string? DiscordData { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastOnline { get; set; }
        public DateTime? IsBanned { get; set; } // хранит дату бана

    }

    public class Subscription
    {
        public enum Status_Subscription { active, inactive, paused, suspended }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // 👈 автоинкремент
        public long SubscriptionId { get; set; }

        // связь с владельцем подписки
        public long AccountId { get; set; }
        public User? Account { get; set; } = null!;

        public Status_Subscription Status { get; set; } = Status_Subscription.inactive; // 👈 дефолт
        public DateTime PurchaseDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime? UnfreezeDate { get; set; }

        // связь с рефералом
        public long? ReferralId { get; set; }
        public User? Referral { get; set; }   // 👈 это навигация к User
    }


    public class Log
    {
        [Key]
        public long LogId { get; set; }
        public long AuthorId { get; set; }
        public string Action { get; set; } = null!;
        public string? Details { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public class Session
    {
        [Key]
        public long SessionId { get; set; }          // автоинкремент
        public long AccountId { get; set; }          // связь с User
        public long UserId { get; set; }             // можно использовать тот же AccountId или отдельный ID
        public DateTime LoginDate { get; set; }      // время авторизации
        public string? IpAddressHash { get; set; }   // хеш IP
        public string? HwidHash { get; set; }        // хеш HWID
    }

    public class Message
    {
        [Key]
        public long MessageId { get; set; }
        public long AuthorId { get; set; }
        public string Text { get; set; } = null!;
        public DateTime Timestamp { get; set; }
    }
}
