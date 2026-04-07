using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZuluIA_Back.Domain.Entities.Ventas;

namespace ZuluIA_Back.Infrastructure.Persistence.Configurations;

public class PresupuestoConfiguration : IEntityTypeConfiguration<Presupuesto>
{
    public void Configure(EntityTypeBuilder<Presupuesto> builder)
    {
        builder.ToTable("presupuestos");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedOnAdd();

        builder.Property(x => x.SucursalId)
               .HasColumnName("sucursal_id").IsRequired();

        builder.Property(x => x.TerceroId)
               .HasColumnName("tercero_id").IsRequired();

        builder.Property(x => x.Fecha)
               .HasColumnName("fecha").IsRequired();

        builder.Property(x => x.FechaVigencia)
               .HasColumnName("fecha_vigencia");

        builder.Property(x => x.MonedaId)
               .HasColumnName("moneda_id").IsRequired();

        builder.Property(x => x.Cotizacion)
               .HasColumnName("cotizacion")
               .HasPrecision(18, 6)
               .HasDefaultValue(1m);

        builder.Property(x => x.Total)
               .HasColumnName("total")
               .HasPrecision(18, 4)
               .HasDefaultValue(0m);

        builder.Property(x => x.Estado)
               .HasColumnName("estado")
               .HasMaxLength(50)
               .HasDefaultValue("PENDIENTE")
               .IsRequired();

        builder.Property(x => x.Observacion)
               .HasColumnName("observacion")
               .HasMaxLength(500);

        builder.Property(x => x.ComprobanteId)
               .HasColumnName("comprobante_id");

        // Audit
        builder.Property(x => x.CreatedAt)
               .HasColumnName("created_at")
               .HasDefaultValueSql("now()");
        builder.Property(x => x.UpdatedAt)
               .HasColumnName("updated_at")
               .HasDefaultValueSql("now()");
        builder.Property(x => x.DeletedAt)
               .HasColumnName("deleted_at");
        builder.Property(x => x.CreatedBy)
               .HasColumnName("created_by");
        builder.Property(x => x.UpdatedBy)
               .HasColumnName("updated_by");

        builder.HasMany(x => x.Items)
               .WithOne()
               .HasForeignKey(x => x.PresupuestoId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
