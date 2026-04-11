using Microsoft.EntityFrameworkCore;
using TrainingAndPlacementPortal.Models;

namespace TrainingAndPlacementPortal.Data
{
    public static class DbSeeder
    {
        public static void Seed(AppDbContext context)
        {
            if (!context.Companies.Any())
            {
                var companies = new List<Company>
                {
                    new Company { CompanyName = "Multiicon", Email = "hr@multiicon.in", JobLocation = "Rajkot", Status = "Verified", ContactName = "Shweta Jethwa", ContactEmail="hr@multiicon.in", RegisteredAt = DateTime.UtcNow },
                    new Company { CompanyName = "Gateway Group", Email = "careers@gateway.in", JobLocation = "Ahmedabad", Status = "Verified", ContactName = "Rahul Desai", ContactEmail="careers@gateway.in", RegisteredAt = DateTime.UtcNow.AddDays(-10) },
                    new Company { CompanyName = "TCS Ltd", Email = "campus@tcs.com", JobLocation = "Bangalore", Status = "Verified", ContactName = "Amit Sharma", ContactEmail="campus@tcs.com", RegisteredAt = DateTime.UtcNow.AddDays(-20) },
                    new Company { CompanyName = "Simform", Email = "hr@simform.com", JobLocation = "Ahmedabad", Status = "Pending", ContactName = "Priya Patel", ContactEmail="hr@simform.com", RegisteredAt = DateTime.UtcNow.AddDays(-2) }
                };
                context.Companies.AddRange(companies);
                context.SaveChanges();
            }

            if (!context.JobPostings.Any())
            {
                var companies = context.Companies.ToList();
                if(companies.Count > 0)
                {
                    var jobs = new List<JobPosting>
                    {
                        new JobPosting { CompanyId = companies[0].Id, JobPosition = "PHP Developer", AnnualCTC = "₹3.6 LPA", JobLocation = "Rajkot", SelectionProcess = "Aptitude + Technical + HR", Status = "Completed", IsActive = true, PostedAt = DateTime.UtcNow.AddDays(-30), CampusDriveDate = DateTime.UtcNow.AddDays(-10) },
                        new JobPosting { CompanyId = companies[1].Id, JobPosition = "Software Engineer", AnnualCTC = "₹4.5 LPA", JobLocation = "Ahmedabad", SelectionProcess = "Written Test + Technical Round + HR", Status = "Approved", IsActive = true, PostedAt = DateTime.UtcNow.AddDays(-5), CampusDriveDate = DateTime.UtcNow.AddDays(5) },
                        new JobPosting { CompanyId = companies[2].Id, JobPosition = "Software Development Engineer", AnnualCTC = "₹8 LPA", JobLocation = "Bangalore", SelectionProcess = "Aptitude Test + Personal Int. + Practical Test", Status = "Approved", IsActive = true, PostedAt = DateTime.UtcNow.AddDays(-15), CampusDriveDate = DateTime.UtcNow.AddDays(1) },
                        new JobPosting { CompanyId = companies[3].Id, JobPosition = "DotNet Developer", AnnualCTC = "₹5.5 LPA", JobLocation = "Ahmedabad", SelectionProcess = "Aptitude + HR", Status = "Pending", IsActive = true, PostedAt = DateTime.UtcNow.AddDays(-1) }
                    };
                    context.JobPostings.AddRange(jobs);
                    context.SaveChanges();
                }
            }

            if (!context.Students.Any())
            {
                // Create 3 users + students
                var users = new List<User>();
                var students = new List<Student>();

                for (int i = 1; i <= 3; i++)
                {
                    var u = new User
                    {
                        Email = $"student{i}@rku.ac.in",
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("Student@123"),
                        Role = "Student",
                        IsApproved = true,
                        CreatedAt = DateTime.UtcNow.AddDays(-i*10)
                    };
                    context.Users.Add(u);
                    context.SaveChanges();

                    var s = new Student
                    {
                        UserId = u.Id,
                        FullName = $"Test Student {i}",
                        EnrollmentNumber = $"26RKU00{i}",
                        Branch = "Computer Science",
                        Semester = "8",
                        CGPA = 8.0 + (i * 0.2),
                        PersonalEmail = $"personal{i}@gmail.com",
                        MobileNumber = $"987654321{i}",
                        RegisteredAt = u.CreatedAt
                    };
                    context.Students.Add(s);
                    context.SaveChanges();
                }
            }

            if (!context.JobApplications.Any())
            {
                var students = context.Students.ToList();
                var jobs = context.JobPostings.ToList();

                if (students.Count > 0 && jobs.Count > 0)
                {
                    var apps = new List<JobApplication>
                    {
                        new JobApplication { StudentId = students[0].Id, JobPostingId = jobs[0].Id, ApplicationStatus = "Selected", AppliedAt = DateTime.UtcNow.AddDays(-20) },
                        new JobApplication { StudentId = students[1].Id, JobPostingId = jobs[0].Id, ApplicationStatus = "Rejected", AppliedAt = DateTime.UtcNow.AddDays(-20) },
                        new JobApplication { StudentId = students[2].Id, JobPostingId = jobs[0].Id, ApplicationStatus = "Selected", AppliedAt = DateTime.UtcNow.AddDays(-20) },
                        new JobApplication { StudentId = students[0].Id, JobPostingId = jobs[1].Id, ApplicationStatus = "Shortlisted", AppliedAt = DateTime.UtcNow.AddDays(-2) },
                        new JobApplication { StudentId = students[1].Id, JobPostingId = jobs[2].Id, ApplicationStatus = "Applied", AppliedAt = DateTime.UtcNow.AddDays(-1) }
                    };
                    context.JobApplications.AddRange(apps);
                    context.SaveChanges();
                }
            }
        }
    }
}
