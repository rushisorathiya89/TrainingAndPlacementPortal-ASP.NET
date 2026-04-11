using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TrainingAndPlacementPortal.Data;
using TrainingAndPlacementPortal.Models;
using System;
using System.Linq;

namespace TrainingAndPlacementPortal.Scripts
{
    public class InterviewSeeder
    {
        public static void Run(IServiceProvider serviceProvider)
        {
            using var context = new AppDbContext(
                serviceProvider.GetRequiredService<DbContextOptions<AppDbContext>>());

            // 0. Ensure Admin exists
            var adminEmail = "admin@rku.ac.in";
            var existingAdmin = context.Users.FirstOrDefault(u => u.Email == adminEmail);
            if (existingAdmin == null)
            {
                var admin = new User
                {
                    Email = adminEmail,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@1234"),
                    Role = "Admin",
                    IsApproved = true,
                    CreatedAt = DateTime.UtcNow
                };
                context.Users.Add(admin);
                context.SaveChanges();
                Console.WriteLine("Test admin created.");
            }

            // 1. Ensure a Company exists
            var company = context.Companies.FirstOrDefault(c => c.Email == "testcompany@rku.ac.in");
            if (company == null)
            {
                company = new Company
                {
                    CompanyName = "Google India",
                    Email = "testcompany@rku.ac.in",
                    Website = "https://google.com",
                    JobLocation = "Bangalore",
                    Description = "Global tech leader.",
                    ContactName = "HR Manager",
                    ContactEmail = "hr@google.com",
                    ContactMobile = "9988776655",
                    Status = "Verified",
                    RegisteredAt = DateTime.UtcNow
                };
                context.Companies.Add(company);
                context.SaveChanges();
            }

            // 2. Ensure an Approved Job Posting exists
            var job = context.JobPostings.FirstOrDefault(j => j.CompanyId == company.Id && j.Status == "Approved");
            if (job == null)
            {
                job = new JobPosting
                {
                    CompanyId = company.Id,
                    JobPosition = "Software Engineer",
                    AnnualCTC = "12 LPA",
                    InternshipDuration = "6 Months",
                    Stipend = "25000",
                    Bond = "None",
                    DateOfJoining = DateTime.UtcNow.AddMonths(3),
                    SelectionProcess = "Aptitude, Tech Interview, HR",
                    Status = "Approved",
                    IsActive = true,
                    PostedAt = DateTime.UtcNow,
                    CampusDriveDate = DateTime.UtcNow.AddDays(7),
                    RegistrationLink = "https://rku.ac.in",
                    AdditionalNotes = "Final year students only.",
                    DocumentsPath = "",
                    EligibleBatches = "2024, 2025",
                    EligibleCourses = "B.Tech, MCA"
                };
                context.JobPostings.Add(job);
                context.SaveChanges();
            }

            // 3. Ensure a Student exists
            var student = context.Students.FirstOrDefault(s => s.EnrollmentNumber == "21SOECE11001");
            if (student == null)
            {
                // Assuming SeedTestData already created one or we create a fallback
                Console.WriteLine("Student not found, please run SeedTestData first.");
                return;
            }

            // 4. Ensure Application exists
            var app = context.JobApplications.FirstOrDefault(a => a.StudentId == student.Id && a.JobPostingId == job.Id);
            if (app == null)
            {
                app = new JobApplication
                {
                    StudentId = student.Id,
                    JobPostingId = job.Id,
                    ApplicationStatus = "Applied",
                    AppliedAt = DateTime.UtcNow
                };
                context.JobApplications.Add(app);
                context.SaveChanges();
                Console.WriteLine("Seeded Application for 'Software Engineer' at 'Google India'.");
            }
            else
            {
                Console.WriteLine("Application already exists.");
            }
        }
    }
}
