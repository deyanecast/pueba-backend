using Microsoft.EntityFrameworkCore;
using MiBackend.Data;
using MiBackend.Interfaces.Repositories;
using MiBackend.Interfaces.Services;
using MiBackend.Repositories;
using MiBackend.Services;
using MiBackend.Strategies;
using DotNetEnv;
using Microsoft.AspNetCore.ResponseCompression;

var builder = WebApplication.CreateBuilder(args);

// Load environment variables from .env file in development
if (builder.Environment.IsDevelopment())
{
    var envFilePath = Path.Combine(Directory.GetCurrentDirectory(), ".env");
    if (File.Exists(envFilePath))
    {
        Env.Load(envFilePath);
        Console.WriteLine("Environment variables loaded from: " + envFilePath);
    }
}

// Get database connection string
var host = Environment.GetEnvironmentVariable("DB_HOST");
var database = Environment.GetEnvironmentVariable("DB_DATABASE");
var username = Environment.GetEnvironmentVariable("DB_USERNAME");
var password = Environment.GetEnvironmentVariable("DB_PASSWORD");
var dbPort = Environment.GetEnvironmentVariable("DB_PORT");

if (string.IsNullOrEmpty(host) || 
    string.IsNullOrEmpty(database) || 
    string.IsNullOrEmpty(username) || 
    string.IsNullOrEmpty(password) || 
    string.IsNullOrEmpty(dbPort))
{
    throw new InvalidOperationException("Missing required environment variables for database connection. " +
        "Make sure to configure: DB_HOST, DB_DATABASE, DB_USERNAME, DB_PASSWORD, DB_PORT");
}

var connectionString = $"Host={host};" +
                      $"Database={database};" +
                      $"Username={username};" +
                      $"Password={password};" +
                      $"Port={dbPort};" +
                      "SSL Mode=Require;" +
                      "Trust Server Certificate=true";

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add Memory Cache
builder.Services.AddMemoryCache(options =>
{
    options.SizeLimit = 1024 * 1024 * 50; // 50MB limit
    options.CompactionPercentage = 0.2; // 20% compaction
    options.ExpirationScanFrequency = TimeSpan.FromMinutes(5);
});

// Add Response Compression
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
        new[] { "application/json" });
});

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Configure DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseNpgsql(connectionString, npgsqlOptions =>
    {
        npgsqlOptions.EnableRetryOnFailure(
            maxRetryCount: 3,
            maxRetryDelay: TimeSpan.FromSeconds(3),
            errorCodesToAdd: null);
        npgsqlOptions.CommandTimeout(15);
        npgsqlOptions.MinBatchSize(1);
        npgsqlOptions.MaxBatchSize(50);
    });
    
    // Optimizaciones de rendimiento
    options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTrackingWithIdentityResolution);
    options.EnableSensitiveDataLogging(builder.Environment.IsDevelopment());
    options.EnableDetailedErrors(builder.Environment.IsDevelopment());
});

// Configure DI
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IProductoService, ProductoService>();
builder.Services.AddScoped<IComboService, ComboService>();
builder.Services.AddScoped<IVentaService, VentaService>();

// Register strategies
builder.Services.AddScoped<ProductoVentaStrategy>();
builder.Services.AddScoped<ComboVentaStrategy>();
builder.Services.AddScoped<IVentaItemStrategy>(sp => sp.GetRequiredService<ProductoVentaStrategy>());
builder.Services.AddScoped<IVentaItemStrategy>(sp => sp.GetRequiredService<ComboVentaStrategy>());

// Configurar logging
builder.Services.AddLogging(logging =>
{
    logging.ClearProviders();
    logging.AddConsole();
    logging.AddDebug();
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseResponseCompression();
app.UseHttpsRedirection();
app.UseCors();
app.UseAuthorization();
app.MapControllers();

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new { 
    status = "healthy", 
    environment = app.Environment.EnvironmentName,
    dbConnectionConfigured = !string.IsNullOrEmpty(connectionString)
}));

app.Run();
