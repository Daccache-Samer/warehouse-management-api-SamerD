using Microsoft.EntityFrameworkCore;
using Notifications.Domain.Notification;

namespace Notifications.Infrastructure.Persistence;

public class NotificationDbContext(DbContextOptions<NotificationDbContext> options) : DbContext(options)
{
    public DbSet<Notification> Notifications => Set<Notification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(n => n.NotificationId);
            entity.HasIndex(n => n.SourceEventId).IsUnique(); 
            entity.Property(n => n.Type).HasConversion<string>().HasMaxLength(50);
            entity.Property(n => n.Severity).HasConversion<string>().HasMaxLength(20);
            entity.Property(n => n.Status).HasConversion<string>().HasMaxLength(20);
            entity.Property(n => n.Title).HasMaxLength(200);
            entity.Property(n => n.RelatedEntityType).HasMaxLength(50);
        });
    }
}