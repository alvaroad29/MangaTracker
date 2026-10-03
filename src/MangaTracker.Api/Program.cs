using System.Security.Claims;
using System.Text;
using System.Threading.RateLimiting;
using CloudinaryDotNet;
using FluentValidation;
using MangaTracker.Api.ExceptionHandling;
using MangaTracker.Api.OpenApi;
using MangaTracker.Application.Interfaces;
using MangaTracker.Infrastructure.AiSearch;
using MangaTracker.Infrastructure.Identity;
using MangaTracker.Infrastructure.Persistence;
using MangaTracker.Infrastructure.Services;
using MangaTracker.Infrastructure.Storage;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Polly;
using Scalar.AspNetCore;
using Tenrai;
using Tenrai.Config;

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
builder.Services.AddOpenApi(options => {
    options.AddDocumentTransformer<JwtBearerDocumentTransformer>();
});

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

// rate limit
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(
        httpContext =>
        {
            var ip = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault()
                     ?? httpContext.Connection.RemoteIpAddress?.ToString()
                     ?? "unknown";
            
            return RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: ip,
                factory: _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 200,
                    Window = TimeSpan.FromMinutes(1),
                    QueueLimit = 0
                });
        });
    
    options.AddPolicy("export", httpContext =>
    {
        var userId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "anon";
        // solo usuarios autenticados llegan al endpoint de export, asi que no importa el "anon"
        
        return RateLimitPartition.GetConcurrencyLimiter( // limita cantidad al mismo tiempo 
            partitionKey: userId,
            factory: _ => new ConcurrencyLimiterOptions
            {
                PermitLimit = 2,  // Max 2 exports simultáneos por usuario
                QueueLimit = 0
            });
    });

    // para login/register/refresh: 10 intentos por minuto, por IP
    options.AddPolicy("auth", httpContext =>
    {
        var ip = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault()
                 ?? httpContext.Connection.RemoteIpAddress?.ToString()
                 ?? "unknown";

        return RateLimitPartition.GetSlidingWindowLimiter(
            partitionKey: ip,
            factory: _ => new SlidingWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(1),
                SegmentsPerWindow = 4,
                QueueLimit = 0 // sin cola: el que se pasa del límite, rechazado directo con 429
            });
    });

    // para la búsqueda con IA: 5 por minuto por IP — protege tu cuota gratis de Gemini
    options.AddPolicy("ai-search", httpContext =>
    {
        var ip = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault()
                 ?? httpContext.Connection.RemoteIpAddress?.ToString()
                 ?? "unknown";

        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: ip,
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            });
    });
    
});

// --- Servicios propios ---
builder.Services.AddScoped<ITokenService, JwtTokenService>();
builder.Services.AddScoped<IRefreshTokenService, RefreshTokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IMangaCatalogService, MangaTracker.Infrastructure.Catalog.TenraiCatalogService>();

builder.Services.AddScoped<UserMapper>();
builder.Services.AddScoped<MangaTracker.Infrastructure.Catalog.CatalogMapper>();

builder.Services.AddScoped<IUserMangaEntryService, MangaTracker.Infrastructure.Entries.UserMangaEntryService>();
builder.Services.AddScoped<IMangaListService, MangaTracker.Infrastructure.Entries.MangaListService>();

builder.Services.AddScoped<IMangaListSocialService, MangaTracker.Infrastructure.Social.MangaListSocialService>();

// --- Cloudinary ----
var cloudinaryAccount = new Account(
    builder.Configuration["Cloudinary:CloudName"],
    builder.Configuration["Cloudinary:ApiKey"],
    builder.Configuration["Cloudinary:ApiSecret"]);

builder.Services.AddSingleton(new Cloudinary(cloudinaryAccount));
builder.Services.AddScoped<IImageStorageService, CloudinaryImageStorageService>();
builder.Services.AddScoped<IMangaSnapshotService, MangaTracker.Infrastructure.Catalog.MangaSnapshotService>();

// --- Cache ---
builder.Services.AddMemoryCache();

// --- Tenrai (catálogo de manga) ---
var tenraiBaseUrl = builder.Configuration["Tenrai:BaseUrl"] ?? "https://api.tenrai.org/v1/";

builder.Services.AddHttpClient("Tenrai", client =>
{
    client.BaseAddress = new Uri(tenraiBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(15);
})
.AddResilienceHandler("tenrai-resilience", resilience => // configurando polly
{
    resilience.AddRetry(new Microsoft.Extensions.Http.Resilience.HttpRetryStrategyOptions
    {
        MaxRetryAttempts = 3,
        BackoffType = Polly.DelayBackoffType.Exponential
    });
    resilience.AddCircuitBreaker(new Microsoft.Extensions.Http.Resilience.HttpCircuitBreakerStrategyOptions
    {
        FailureRatio = 0.5,
        SamplingDuration = TimeSpan.FromSeconds(30),
        BreakDuration = TimeSpan.FromSeconds(30)
    });
});

builder.Services.AddSingleton<ITenrai>(sp => // configurando el servicio, distinto de antes
{
    var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
    var httpClient = httpClientFactory.CreateClient("Tenrai");

    var config = new TenraiClientConfiguration
    {
        LimiterConfigurations = TaskLimiterConfiguration.Default // 60/min, 3/seg — mismo límite que la API pública
    };

    return new TenraiClient(config, httpClient);
});

// --- gemini ---
var geminiBaseUrl = builder.Configuration["Gemini:BaseUrl"]!;
var geminiApiKey = builder.Configuration["Gemini:ApiKey"];

builder.Services.AddHttpClient<IAiSearchTranslationService, GeminiAiSearchTranslationService>(client =>
{
    client.BaseAddress = new Uri(geminiBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(20);
    client.DefaultRequestHeaders.Add("x-goog-api-key", geminiApiKey);
});

builder.Services.AddScoped<IAiSearchService, AiSearchService>();

// -- backup
builder.Services.AddScoped<IBackupService, MangaTracker.Infrastructure.Backup.BackupService>();


var allowedOrigins = builder.Configuration.GetSection("Frontend:AllowedOrigins").Get<string[]>() ?? [];

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod());
});

var app = builder.Build();

app.UseExceptionHandler(); // primero de todoo, para capturar cualquier error de lo que sigue

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    { 
        options
        .WithTitle("MangaTracker API")
        .WithTheme(ScalarTheme.Saturn) // Temas disponibles: Default, Alternate, Moon, Purple, Solarized, BluePlanet, DeepSpace, Saturn, Kepler, Mars, None
        .WithDarkMode(true)
        .WithPreferredScheme("Bearer") // Nombre del esquema que definimos en el transformador
        .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient); // Cliente por defect
    });
}

app.UseHttpsRedirection();
app.UseCors("Frontend");
app.UseRateLimiter();   
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();