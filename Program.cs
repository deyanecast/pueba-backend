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

// Configuración del puerto
Console.WriteLine($"Ambiente detectado: {builder.Environment.EnvironmentName}");
Console.WriteLine($"Variable ASPNETCORE_ENVIRONMENT: {Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}");

string port;
if (builder.Environment.IsDevelopment())
{
    port = "5001";
    Console.WriteLine("Ambiente de desarrollo detectado, usando puerto 5001");
    builder.WebHost.UseUrls($"http://localhost:{port}", $"http://0.0.0.0:{port}");
}
else
{
    port = Environment.GetEnvironmentVariable("PORT") ?? "5038";
    Console.WriteLine($"Ambiente de producción detectado, usando puerto {port}");
    builder.WebHost.UseUrls($"http://+:{port}");
}

Console.WriteLine($"URLs configuradas: {string.Join(", ", builder.WebHost.GetSetting(WebHostDefaults.ServerUrlsKey))}");

// Obtener la cadena de conexión
var connectionString = "Host=aws-0-us-west-1.pooler.supabase.com;" +
                      "Database=postgres;" +
                      "Username=postgres.bgyeganjxxjzrogodwes;" +
                      "Password=LIh^dxDg6@X0W&;" +
                      "Port=6543;" +
                      "SSL Mode=Require;" +
                      "Trust Server Certificate=true";

Console.WriteLine("Usando cadena de conexión con transaction pooler para pruebas");
Console.WriteLine($"Ambiente: {builder.Environment.EnvironmentName}");

// Configuración de servicios
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configuración de CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Configuración de la base de datos
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    Console.WriteLine("Configurando conexión a la base de datos...");
    options.UseNpgsql(connectionString, npgsqlOptions =>
    {
        npgsqlOptions.EnableRetryOnFailure(
            maxRetryCount: 3,
            maxRetryDelay: TimeSpan.FromSeconds(5),
            errorCodesToAdd: null);
    });
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
        logger.LogInformation("Obteniendo lista de productos...");
        var productos = await db.Productos.ToListAsync();
        logger.LogInformation($"Se encontraron {productos.Count} productos");
        return Results.Ok(productos);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error al obtener productos");
        return Results.Problem(
            title: "Error al obtener productos",
            detail: ex.Message + (ex.InnerException != null ? " - " + ex.InnerException.Message : ""),
            statusCode: 500
        );
    }
});

app.MapGet("/api/productos/{id}", async (int id, ApplicationDbContext db, ILogger<Program> logger) =>
{
    try
    {
        logger.LogInformation($"Buscando producto con ID: {id}");
        var producto = await db.Productos.FindAsync(id);
        if (producto == null)
        {
            logger.LogWarning($"Producto con ID {id} no encontrado");
            return Results.NotFound($"Producto con ID {id} no encontrado");
        }
        return Results.Ok(producto);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, $"Error al obtener producto con ID {id}");
        return Results.Problem(
            title: "Error al obtener producto",
            detail: ex.Message + (ex.InnerException != null ? " - " + ex.InnerException.Message : ""),
            statusCode: 500
        );
    }
});

app.MapPost("/api/productos/test", async (ApplicationDbContext db, ILogger<Program> logger) =>
{
    try
    {
        logger.LogInformation("Creando producto de prueba...");
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
        logger.LogError(ex, "Error al crear producto de prueba");
        return Results.Problem(
            title: "Error al crear producto",
            detail: ex.Message + (ex.InnerException != null ? " - " + ex.InnerException.Message : ""),
            statusCode: 500
        );
    }
});

app.Run();
