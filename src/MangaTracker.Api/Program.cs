using System.Text;
using FluentValidation;
using MangaTracker.Api.ExceptionHandling;
using MangaTracker.Application.Interfaces;
using MangaTracker.Infrastructure.Identity;
using MangaTracker.Infrastructure.Persistence;
using MangaTracker.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// --- Manejo global de errores ---
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();


// --- Controllers + validación ---
builder.Services.AddControllers(options =>
{
    options.Filters.Add<MangaTracker.Api.Filters.ValidationFilter>();
});

builder.Services.AddValidatorsFromAssembly(typeof(MangaTracker.Application.AssemblyMarker).Assembly);


// --- OpenAPI ---
builder.Services.AddOpenApi();

// --- Base de datos ---
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


// --- Identity ---
builder.Services.AddDataProtection();

builder.Services
    .AddIdentityCore<ApplicationUser>(options =>
    {
        // --- Contraseña ---
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = false; // caracter raro
        options.Password.RequiredLength = 8;
        options.Password.RequiredUniqueChars = 1;

        // --- Lockout (protección contra fuerza bruta) ---
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.AllowedForNewUsers = true;

        // --- Usuario ---
        options.User.RequireUniqueEmail = true;

        // --- Sign-in ---
        options.SignIn.RequireConfirmedEmail = false; // pasalo a true si en el futuro agregás confirmación por mail
    })
    .AddRoles<IdentityRole<Guid>>()
    .AddSignInManager()  
    .AddDefaultTokenProviders()
    .AddEntityFrameworkStores<ApplicationDbContext>();

// --- JWT ---
var jwtSecret = builder.Configuration["Jwt:Secret"]!;

builder.Services.AddAuthentication(
        JwtBearerDefaults.AuthenticationScheme
        )
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });

builder.Services.AddAuthorization();

// --- Servicios propios ---
builder.Services.AddScoped<ITokenService, JwtTokenService>();
builder.Services.AddScoped<IRefreshTokenService, RefreshTokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<UserMapper>();

var app = builder.Build();

app.UseExceptionHandler(); // primero de todoo, para capturar cualquier error de lo que sigue

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();