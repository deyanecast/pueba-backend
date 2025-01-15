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

            modelBuilder.Entity<ComboDetalle>(entity =>
            {
                entity.ToTable("combo_detalles");
                
                entity.HasKey(e => e.ComboDetalleId)
                    .HasName("combo_detalles_pkey");

                entity.Property(e => e.ComboDetalleId)
                    .HasColumnName("combo_detalle_id")
                    .UseIdentityAlwaysColumn();

                entity.Property(e => e.ComboId)
                    .HasColumnName("combo_id");

                entity.Property(e => e.ProductoId)
                    .HasColumnName("producto_id");

                entity.Property(e => e.CantidadLibras)
                    .HasColumnName("cantidad_libras");

                entity.HasOne(d => d.Combo)
                    .WithMany(p => p.ComboDetalles)
                    .HasForeignKey(d => d.ComboId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(d => d.Producto)
                    .WithMany(p => p.ComboDetalles)
                    .HasForeignKey(d => d.ProductoId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configuración de índices para optimizar consultas
            modelBuilder.Entity<Venta>()
                .HasIndex(v => v.FechaVenta);

            modelBuilder.Entity<VentaDetalle>()
                .HasIndex(vd => new { vd.VentaId, vd.TipoItem });

            modelBuilder.Entity<Producto>()
                .HasIndex(p => p.EstaActivo);

            modelBuilder.Entity<Combo>()
                .HasIndex(c => c.EstaActivo);

            modelBuilder.Entity<ComboDetalle>()
                .HasIndex(cd => new { cd.ComboId, cd.ProductoId });

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