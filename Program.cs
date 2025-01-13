using Microsoft.EntityFrameworkCore;
using MiBackend.Data;
using MiBackend.Models;

var builder = WebApplication.CreateBuilder(args);

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

// Get connection string from configuration
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
Console.WriteLine($"Connection string loaded: {(string.IsNullOrEmpty(connectionString) ? "No" : "Yes")}");

if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("DefaultConnection string is not configured");
}

// Add DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseNpgsql(connectionString);
    
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

// Endpoints
app.MapGet("/api/productos", async (ApplicationDbContext db) =>
{
    try
    {
        var productos = await db.Productos.ToListAsync();
        return Results.Ok(productos);
    }
    catch (Exception ex)
    {
        return Results.Problem(
            title: "Error al obtener productos",
            detail: ex.Message,
            statusCode: 500
        );
    }
});

app.MapGet("/api/productos/{id}", async (int id, ApplicationDbContext db) =>
{
    try
    {
        var producto = await db.Productos.FindAsync(id);
        if (producto == null)
        {
            return Results.NotFound();
        }
        return Results.Ok(producto);
    }
    catch (Exception ex)
    {
        return Results.Problem(
            title: "Error al obtener el producto",
            detail: ex.Message,
            statusCode: 500
        );
    }
});

app.MapPost("/api/productos/test", async (ApplicationDbContext db) =>
{
    try
    {
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

        return Results.Ok(producto);
    }
    catch (Exception ex)
    {
        return Results.Problem(
            title: "Error al crear el producto de prueba",
            detail: ex.Message,
            statusCode: 500
        );
    }
});

app.Run();
