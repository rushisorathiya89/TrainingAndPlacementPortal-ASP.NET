using Microsoft.EntityFrameworkCore;
using TrainingAndPlacementPortal.Models;

namespace TrainingAndPlacementPortal.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<JobPosting> JobPostings { get; set; }
        public DbSet<JobApplication> JobApplications { get; set; }
        public DbSet<InterviewSchedule> InterviewSchedules { get; set; }
        public DbSet<Payment> Payments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Unique index on User.Email
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // 1:1 User <-> Student relationship
            modelBuilder.Entity<User>()
                .HasOne(u => u.Student)
                .WithOne(s => s.User)
                .HasForeignKey<Student>(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Seed Admin User
            // Password: Admin@123 hashed with BCrypt
            var adminPasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123");

            modelBuilder.Entity<User>().HasData(new User
            {
                Id = 1,
                Email = "admin@rku.ac.in",
                PasswordHash = adminPasswordHash,
                Role = "Admin",
                IsApproved = true,
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            });
        }
    }
}
