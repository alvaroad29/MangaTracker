using FluentValidation;
using MangaTracker.Infrastructure.Identity;
using MangaTracker.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);



builder.Services.AddControllers(options =>
{
    options.Filters.Add<MangaTracker.Api.Filters.ValidationFilter>();
});

builder.Services.AddOpenApi();

builder.Services.AddValidatorsFromAssembly(typeof(MangaTracker.Application.AssemblyMarker).Assembly);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services
    .AddIdentityCore<ApplicationUser>(options =>
    {
        // --- Contraseña ---
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = false; // ajustable a gusto
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

builder.Services.AddDataProtection();

var app = builder.Build();


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