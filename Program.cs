using System.Collections.Generic;
using System.Text;
using AssignmentApi.Data;
using AssignmentApi.Hubs;
using AssignmentApi.Middleware;
using AssignmentApi.Repositories;
using AssignmentApi.Services;
using AssignmentApi.BackgroundServices;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add Database Context (using localdb for default local database creation)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? "Server=(localdb)\\mssqllocaldb;Database=RideMatchingDb;Trusted_Connection=True;MultipleActiveResultSets=true";
builder.Services.AddDbContext<AssignmentDbContext>(options =>
    options.UseSqlServer(connectionString));

// Add Dependency Injection Services (Scoped to keep the project light)
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IMasterService, MasterService>();
builder.Services.AddScoped<IRideService, RideService>();
builder.Services.AddScoped<IDriverRepository, DriverRepository>();
builder.Services.AddScoped<IDriverService, DriverService>();
builder.Services.AddScoped<IRiderRepository, RiderRepository>();
builder.Services.AddScoped<IRiderService, RiderService>();

// Add Hosted Background Service for Ride Matching
builder.Services.AddHostedService<RideMatchingBackgroundService>();

// Add SignalR for Real-time updates
builder.Services.AddSignalR();

// Add Controllers
builder.Services.AddControllers();

// Configure JWT Authentication
var jwtSecret = builder.Configuration["JwtSettings:Secret"] ?? "SuperSecretKeyForRideMatchingSystemTokenGeneration12345";
var key = Encoding.ASCII.GetBytes(jwtSecret);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false
    };
});

// Configure Swagger with Bearer Token Authorization Input
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Real-Time Ride Matching API", Version = "v1" });
    
    // Add Security Definition for JWT
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header
            },
            new List<string>()
        }
    });
});

var app = builder.Build();

// Enable Swagger UI and map it to root (/) for quick access
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Ride Matching API v1");
    c.RoutePrefix = string.Empty; // Swagger UI served at the application root
});

app.UseRouting();

// Enable CORS
app.UseCors(x => x
    .AllowAnyMethod()
    .AllowAnyHeader()
    .SetIsOriginAllowed(origin => true)
    .AllowCredentials());

app.UseAuthentication();

// Custom Session Validation Middleware (Must run AFTER UseAuthentication)
app.UseMiddleware<SessionValidationMiddleware>();

app.UseAuthorization();

app.MapControllers();
app.MapHub<RideHub>("/ridehub");

app.Run();
