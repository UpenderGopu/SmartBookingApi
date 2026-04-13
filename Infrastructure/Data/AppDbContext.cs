using Microsoft.EntityFrameworkCore;
using SmartBookingApi.Core.Entities;

namespace SmartBookingApi.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        // Constructor that passes options to the base class
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
        // Your DbSets will become your database tables
        public DbSet<User> Users { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<Booking> Bookings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // --- Configure User Table ---
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);                              // Id is the primary key
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);  // Name is required, max 100 chars
                entity.Property(e => e.Email).IsRequired().HasMaxLength(150); // Email is required, max 150 chars
                entity.HasIndex(e => e.Email).IsUnique();              // No two users can have same email
            });

            // --- Configure Room Table ---
            modelBuilder.Entity<Room>(entity =>
            {
                entity.HasKey(e => e.Id);                              // Id is the Primary Key
                entity.Property(e => e.Name).IsRequired().HasMaxLength(50); // Room name max 50 chars
                entity.HasIndex(e => e.Name).IsUnique();              // No two rooms can have the same name
                entity.Property(e => e.Capacity).IsRequired();        // Capacity must be provided
            });

            // --- Configure Booking Table ---
            modelBuilder.Entity<Booking>(entity =>
            {
                entity.HasKey(e => e.Id);                     // Id is the Primary Key
                entity.Property(e => e.StartTime).IsRequired(); // Booking must have a start time
                entity.Property(e => e.EndTime).IsRequired();   // Booking must have an end time
                // A Booking belongs to ONE Room. If that Room is deleted, delete its bookings too.
                entity.HasOne(b => b.Room)
                    .WithMany()
                    .HasForeignKey(b => b.RoomId)
                    .OnDelete(DeleteBehavior.Cascade);
                // A Booking belongs to ONE User. If that User is deleted, delete their bookings too.
                entity.HasOne(b => b.User)
                    .WithMany()
                    .HasForeignKey(b => b.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
