using Microsoft.EntityFrameworkCore;
using MiBackend.Models;

namespace MiBackend.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Producto> Productos { get; set; }
        public DbSet<Combo> Combos { get; set; }
        public DbSet<ComboDetalle> ComboDetalles { get; set; }
        public DbSet<Venta> Ventas { get; set; }
        public DbSet<VentaItem> VentaItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Configurar las propiedades decimales
            modelBuilder.Entity<Producto>()
                .Property(p => p.PrecioPorLibra)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Producto>()
                .Property(p => p.CantidadLibras)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Combo>()
                .Property(c => c.Precio)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<VentaItem>()
                .Property(v => v.PrecioUnitario)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Venta>()
                .Property(v => v.MontoTotal)
                .HasColumnType("decimal(18,2)");
        }
    }
} 