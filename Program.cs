using Microsoft.EntityFrameworkCore;
using MiBackend.Data;
using MiBackend.Models;
using DotNetEnv;
using Polly;

var builder = WebApplication.CreateBuilder(args);

// Cargar variables de entorno desde el archivo .env solo en desarrollo
if (builder.Environment.IsDevelopment())
{
    var envFilePath = Path.Combine(Directory.GetCurrentDirectory(), ".env");
    if (File.Exists(envFilePath))
    {
        Env.Load(envFilePath);
        Console.WriteLine("Variables de entorno cargadas desde: " + envFilePath);
    }
}

// Configurar el puerto
string port = Environment.GetEnvironmentVariable("PORT") ?? "5038"; // Siempre intentar usar PORT de las variables de entorno primero
if (builder.Environment.IsDevelopment())
{
    port = "5001"; // Sobrescribir puerto solo en desarrollo
    Console.WriteLine("Ejecutando en modo desarrollo, usando puerto 5001");
}
else
{
    Console.WriteLine($"Ejecutando en modo producción, usando puerto {port}");
}

builder.WebHost.UseUrls($"http://*:{port}");
Console.WriteLine($"Configurado para escuchar en el puerto: {port}");

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

// Get connection string from environment variable or configuration
var connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING") 
    ?? builder.Configuration.GetConnectionString("DefaultConnection");

Console.WriteLine($"Connection string source: {(Environment.GetEnvironmentVariable("CONNECTION_STRING") != null ? "Environment Variable" : "Configuration")}");
Console.WriteLine($"Connection string loaded: {(string.IsNullOrEmpty(connectionString) ? "No" : "Yes")}");

if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("No se encontró la cadena de conexión en las variables de entorno ni en la configuración");
}

// Add DbContext with retry policy
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseNpgsql(connectionString, npgsqlOptions =>
    {
        npgsqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorCodesToAdd: null);
    });
    
    // Solo habilitar el seguimiento detallado en desarrollo
    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
    }
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new { 
    status = "healthy", 
    port = port,
    environment = app.Environment.EnvironmentName,
    dbConnectionConfigured = !string.IsNullOrEmpty(connectionString),
    connectionStringSource = Environment.GetEnvironmentVariable("CONNECTION_STRING") != null ? "Environment Variable" : "Configuration"
}));

// Endpoints
app.MapGet("/api/productos", async (ApplicationDbContext db, ILogger<Program> logger) =>
{
    try
    {
        logger.LogInformation("Intentando obtener productos de la base de datos");
        var productos = await db.Productos.ToListAsync();
        logger.LogInformation($"Se obtuvieron {productos.Count} productos");
        return Results.Ok(productos);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error al obtener productos");
        return Results.Problem(
            title: "Error al obtener productos",
            detail: $"Error: {ex.Message}. Inner Exception: {ex.InnerException?.Message}",
            statusCode: 500
        );
    }
});

app.MapGet("/api/productos/{id}", async (int id, ApplicationDbContext db, ILogger<Program> logger) =>
{
    try
    {
        logger.LogInformation($"Intentando obtener producto con ID: {id}");
        var producto = await db.Productos.FindAsync(id);
        if (producto == null)
        {
            logger.LogWarning($"No se encontró el producto con ID: {id}");
            return Results.NotFound();
        }
        return Results.Ok(producto);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, $"Error al obtener el producto con ID: {id}");
        return Results.Problem(
            title: "Error al obtener el producto",
            detail: $"Error: {ex.Message}. Inner Exception: {ex.InnerException?.Message}",
            statusCode: 500
        );
    }
});

app.MapPost("/api/productos/test", async (ApplicationDbContext db, ILogger<Program> logger) =>
{
    try
    {
        logger.LogInformation("Intentando crear producto de prueba");
        var producto = new Producto
        {
            Nombre = "Camarón Jumbo",
            CantidadLibras = 2.5m,
            PrecioPorLibra = 25.99m,
            TipoEmpaque = "Caja 5 libras",
            EstaActivo = true
        };

        db.Productos.Add(producto);
        await db.SaveChangesAsync();
        logger.LogInformation($"Producto de prueba creado con ID: {producto.ProductoId}");
        return Results.Ok(producto);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error al crear el producto de prueba");
        return Results.Problem(
            title: "Error al crear el producto de prueba",
            detail: $"Error: {ex.Message}. Inner Exception: {ex.InnerException?.Message}",
            statusCode: 500
        );
    }
});

app.Run();
