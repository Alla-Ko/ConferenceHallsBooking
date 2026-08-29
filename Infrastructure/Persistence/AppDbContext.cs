using ConferenceHalls.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ConferenceHalls.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<HallService> HallServices { get; set; }

    public DbSet<ConferenceHall> ConferenceHalls { get; set; }

    public DbSet<Booking> Bookings { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ConferenceHall>(entity =>
        {
            entity.Property(e => e.HourlyRate).HasPrecision(18, 2);

            entity.HasMany(h => h.AvailableServices)
                .WithMany(s => s.ConferenceHalls)
                .UsingEntity<Dictionary<string, object>>(
                    "ConferenceHallServices",
                    right => right
                        .HasOne<HallService>()
                        .WithMany()
                        .HasForeignKey("HallServiceId")
                        .OnDelete(DeleteBehavior.Cascade),
                    left => left
                        .HasOne<ConferenceHall>()
                        .WithMany()
                        .HasForeignKey("ConferenceHallId")
                        .OnDelete(DeleteBehavior.Cascade));
        });

        modelBuilder.Entity<Booking>(entity =>
        {
            entity.Property(e => e.HallHourlyRateSnapshot).HasPrecision(18, 2);
            entity.Property(e => e.HallRentalPrice).HasPrecision(18, 2);
            entity.Property(e => e.ServicesPrice).HasPrecision(18, 2);
            entity.Property(e => e.TotalPrice).HasPrecision(18, 2);
            entity.Property(e => e.StartAt).HasColumnType("TEXT");
            entity.Property(e => e.EndAt).HasColumnType("TEXT");

            entity.HasOne(e => e.User)
                .WithMany(u => u.Bookings)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.ConferenceHall)
                .WithMany(h => h.Bookings)
                .HasForeignKey(e => e.ConferenceHallId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(e => e.Services)
                .WithOne(s => s.Booking)
                .HasForeignKey(s => s.BookingId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.ConferenceHallId, e.Status });
        });

        modelBuilder.Entity<BookingServiceItem>(entity =>
        {
            entity.HasKey(e => new { e.BookingId, e.HallServiceId });
            entity.Property(e => e.Price).HasPrecision(18, 2);
        });
    }
}
