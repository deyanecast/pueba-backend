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
        public DbSet<VentaDetalle> VentaDetalles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuración de relaciones y restricciones
            modelBuilder.Entity<VentaDetalle>()
                .HasOne(vd => vd.Venta)
                .WithMany(v => v.VentaDetalles)
                .HasForeignKey(vd => vd.VentaId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<VentaDetalle>()
                .HasOne(vd => vd.Producto)
                .WithMany(p => p.VentaDetalles)
                .HasForeignKey(vd => vd.ProductoId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<VentaDetalle>()
                .HasOne(vd => vd.Combo)
                .WithMany(c => c.VentaDetalles)
                .HasForeignKey(vd => vd.ComboId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ComboDetalle>()
                .HasOne(cd => cd.Combo)
                .WithMany(c => c.ComboDetalles)
                .HasForeignKey(cd => cd.ComboId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ComboDetalle>()
                .HasOne(cd => cd.Producto)
                .WithMany(p => p.ComboDetalles)
                .HasForeignKey(cd => cd.ProductoId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configuración de checks y validaciones
            modelBuilder.Entity<VentaDetalle>()
                .ToTable(tb => tb.HasCheckConstraint(
                    "CK_VentaDetalle_TipoItem",
                    "(tipo_item = 'PRODUCTO' AND producto_id IS NOT NULL AND combo_id IS NULL) OR " +
                    "(tipo_item = 'COMBO' AND combo_id IS NOT NULL AND producto_id IS NULL)"));

            modelBuilder.Entity<Producto>()
                .ToTable(tb => tb.HasCheckConstraint(
                    "CK_Producto_CantidadLibras",
                    "\"CantidadLibras\" >= 0"));

            modelBuilder.Entity<Producto>()
                .ToTable(tb => tb.HasCheckConstraint(
                    "CK_Producto_PrecioPorLibra",
                    "\"PrecioPorLibra\" >= 0"));

            modelBuilder.Entity<Combo>()
                .ToTable(tb => tb.HasCheckConstraint(
                    "CK_Combo_Precio",
                    "\"Precio\" >= 0"));
        }
    }
} 