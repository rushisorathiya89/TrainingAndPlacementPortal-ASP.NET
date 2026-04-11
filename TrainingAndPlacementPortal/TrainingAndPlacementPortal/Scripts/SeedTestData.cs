using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TrainingAndPlacementPortal.Data;
using TrainingAndPlacementPortal.Models;
using System;
using System.Linq;

namespace TrainingAndPlacementPortal.Scripts
{
    public class SeedTestData
    {
        public static void Run(IServiceProvider serviceProvider)
        {
            using var context = new AppDbContext(
                serviceProvider.GetRequiredService<DbContextOptions<AppDbContext>>());

            // 1. Ensure a test student exists
            var testEmail = "student@rku.ac.in";
            var existingUser = context.Users.FirstOrDefault(u => u.Email == testEmail);

            if (existingUser == null)
            {
                var passwordHash = BCrypt.Net.BCrypt.HashPassword("Student@123");
                var user = new User
                {
                    Email = testEmail,
                    PasswordHash = passwordHash,
                    Role = "Student",
                    IsApproved = true, // Auto-approve for testing
                    CreatedAt = DateTime.UtcNow
                };
                context.Users.Add(user);
                context.SaveChanges();

                var student = new Student
                {
                    UserId = user.Id,
                    EnrollmentNumber = "21SOECE11001",
                    FullName = "Rahul Sharma",
                    Branch = "Computer Engineering",
                    Semester = "6th",
                    PersonalEmail = "rahul@gmail.com",
                    MobileNumber = "9876543210",
                    TenthPercentage = 85.0,
                    TwelfthPercentage = 82.0,
                    CGPA = 8.5,
                    PaymentStatus = "Paid", // Bypass payment check
                    PaymentAmount = 500,
                    IsApproved = true,
                    RegisteredAt = DateTime.UtcNow
                };
                context.Students.Add(student);
                context.SaveChanges();
                Console.WriteLine("Test student 'Rahul Sharma' created successfully.");
            }
            else
            {
                Console.WriteLine("Test student already exists.");
                // Ensure it's approved
                existingUser.IsApproved = true;
                if (existingUser.Student != null) {
                    existingUser.Student.IsApproved = true;
                    existingUser.Student.PaymentStatus = "Paid";
                }
                context.SaveChanges();
            }
        }
    }
}
