using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using TrainingAndPlacementPortal.Data;
using TrainingAndPlacementPortal.Services;
using TrainingAndPlacementPortal.Scripts;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Register DbContext with SQL Server. Allow overriding the connection string via environment variables
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
// Support common env var forms: ConnectionStrings__DefaultConnection or DEFAULT_CONNECTION
var envConn = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
           ?? Environment.GetEnvironmentVariable("DEFAULT_CONNECTION");
if (!string.IsNullOrEmpty(envConn))
{
    connectionString = envConn;
    Console.WriteLine("Using database connection from environment variable.");
}
else
{
    Console.WriteLine("Using database connection from configuration (appsettings).");
}

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

// Register JWT Token Service
builder.Services.AddScoped<JwtTokenService>();

// Register Razorpay Service
builder.Services.AddScoped<RazorpayService>();

// Configure JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]!);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
else
{
    // Run seeders in development
    using (var scope = app.Services.CreateScope())
    {
        // Ensure database schema is up-to-date before running seeders
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.Migrate();

        // Run seeders but don't let them crash the app if something goes wrong
        try
        {
            SeedTestData.Run(scope.ServiceProvider);
            InterviewSeeder.Run(scope.ServiceProvider);
        }
        catch (Exception ex)
        {
            // Log and continue. In development you can inspect the exception details in the console.
            Console.WriteLine("Seeder error: " + ex.Message);
        }
    }
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();