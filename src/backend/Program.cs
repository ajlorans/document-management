using LegalDocManagement.API.Data;
using LegalDocManagement.API.Data.Models;
using LegalDocManagement.API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// 1. Read Connection String
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// 2. Add DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// 3. Add Identity
builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
    {
        // Configure identity options here if needed (e.g., password requirements)
        options.SignIn.RequireConfirmedAccount = false; // Set to true for email confirmation
        options.Password.RequireDigit = true;
        options.Password.RequiredLength = 6;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = true;
        options.Password.RequireLowercase = false;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders(); // Needed for things like password reset tokens

// 4. Add Custom Services (Token Service)
builder.Services.AddScoped<ITokenService, TokenService>();

// 5. Add Authentication and JWT Configuration
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] 
            ?? throw new InvalidOperationException("JWT Key not found"))),
        ClockSkew = TimeSpan.Zero // Optional: Adjust tolerance for token expiration
    };
});

// Define a specific CORS policy
var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
                      policy =>
                      {
                          policy.WithOrigins("http://localhost:5173") // Frontend origin
                                .AllowAnyHeader()
                                .AllowAnyMethod();
                      });
});

// Important: Ensure AddAuthorization is added if not already present implicitly
// builder.Services.AddAuthorization(); 

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Seed the database
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>(); // Get logger
    try
    {
        var configuration = services.GetRequiredService<IConfiguration>(); // Get configuration
        // Use Task.Run().Wait() for async operations in startup configuration (or make main async)
        // Using .Result or .Wait() can lead to deadlocks in some contexts, but is often okay here.
        // A better approach for .NET 6+ is often top-level async main, but this works.
        Task.Run(() => SeedData.Initialize(services, configuration)).Wait(); 
        logger.LogInformation("Database seeding completed successfully.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred seeding the DB.");
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    // In development, you might want a more permissive CORS policy
    // or ensure your specific origin policy is correctly applied.
}

app.UseHttpsRedirection();

// Use CORS - before Authentication and Authorization
app.UseCors(MyAllowSpecificOrigins);

// 6. Add Authentication Middleware (BEFORE Authorization)
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
