using API.Entities;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class DataContext : DbContext
{
    public DataContext(DbContextOptions options) : base(options)
    {

    }

    public DbSet<AppUser> Users { get; set; }
    public DbSet<UserLike> Likes { get; set; }
    public DbSet<Message> Messages { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<UserLike>()
            .HasKey(u => new {u.SourceUserId, u.TargetUserId});

        builder.Entity<UserLike>()
            .HasOne(u => u.SourceUser)
            .WithMany(u => u.LikedUsers)
            .HasForeignKey(u => u.SourceUserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<UserLike>()
            .HasOne(u => u.TargetUser)
            .WithMany(u => u.LikedByUsers)
            .HasForeignKey(u => u.TargetUserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Message>()
            .HasOne(m => m.Recipient)
            .WithMany(m => m.ReceivedMessages)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Message>()
            .HasOne(m => m.Sender)
            .WithMany(m => m.SentMessages)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
