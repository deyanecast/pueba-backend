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

// Configure the connection string with user secrets
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
connectionString = connectionString?.Replace("${ConnectionStrings:SupabasePassword}", 
    builder.Configuration["ConnectionStrings:SupabasePassword"] ?? throw new InvalidOperationException("Database password not found in user secrets."));

// Add DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors();

// Endpoint para obtener todos los productos
app.MapGet("/api/productos", async (ApplicationDbContext db) =>
{
    try 
    {
        var productos = await db.Productos.ToListAsync();
        return Results.Ok(productos);
    }
    catch (Exception ex)
    {
        return Results.Problem($"Error al obtener productos: {ex.Message}");
    }
})
.WithName("GetProductos")
.WithOpenApi();

// Endpoint para crear un producto
app.MapPost("/api/productos", async (Producto producto, ApplicationDbContext db) =>
{
    try 
    {
        if (producto == null)
            return Results.BadRequest("El producto no puede ser nulo");

        // No permitimos que se envíe el ID, se generará automáticamente
        producto.ProductoId = 0;
        
        db.Productos.Add(producto);
        await db.SaveChangesAsync();
        return Results.Created($"/api/productos/{producto.ProductoId}", producto);
    }
    catch (Exception ex)
    {
        return Results.Problem($"Error al crear producto: {ex.Message}");
    }
})
.WithName("CreateProducto")
.WithOpenApi();

// Endpoint para obtener un producto por ID
app.MapGet("/api/productos/{id}", async (int id, ApplicationDbContext db) =>
{
    try 
    {
        var producto = await db.Productos.FindAsync(id);
        return producto is null ? Results.NotFound() : Results.Ok(producto);
    }
    catch (Exception ex)
    {
        return Results.Problem($"Error al obtener producto: {ex.Message}");
    }
})
.WithName("GetProductoById")
.WithOpenApi();

// Endpoint para obtener todos los combos con sus detalles
app.MapGet("/api/combos", async (ApplicationDbContext db) =>
{
    var combos = await db.Combos
        .Include(c => c.ComboDetalles)
            .ThenInclude(cd => cd.Producto)
        .ToListAsync();
    return Results.Ok(combos);
});

// Endpoint para obtener todas las ventas con sus detalles
app.MapGet("/api/ventas", async (ApplicationDbContext db) =>
{
    var ventas = await db.Ventas
        .Include(v => v.DetalleVentas)
            .ThenInclude(dv => dv.Producto)
        .Include(v => v.DetalleVentas)
            .ThenInclude(dv => dv.Combo)
        .ToListAsync();
    return Results.Ok(ventas);
});

app.Run();
