using Microsoft.EntityFrameworkCore;
using MiBackend.Data;
using MiBackend.Interfaces;
using MiBackend.Interfaces.Services;
using MiBackend.Repositories;
using MiBackend.Services;
using MiBackend.Strategies;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add Memory Cache
builder.Services.AddMemoryCache();

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
var connectionString = $"Host={builder.Configuration["DB_HOST"]};Database={builder.Configuration["DB_DATABASE"]};Username={builder.Configuration["DB_USERNAME"]};Password={builder.Configuration["DB_PASSWORD"]};Port={builder.Configuration["DB_PORT"]}";

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// Register Unit of Work
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Register Services
builder.Services.AddScoped<IProductoService, ProductoService>();
builder.Services.AddScoped<IComboService, ComboService>();
builder.Services.AddScoped<IVentaService, VentaService>();

// Register Strategies
builder.Services.AddScoped<ProductoVentaStrategy>();
builder.Services.AddScoped<ComboVentaStrategy>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors();
app.UseAuthorization();
app.MapControllers();

app.Run();
