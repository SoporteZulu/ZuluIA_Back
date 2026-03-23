using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZuluIA_Back.Domain.Entities.Logistica;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Infrastructure.Persistence.Configurations;

public class OrdenPreparacionConfiguration : IEntityTypeConfiguration<OrdenPreparacion>
{
    public void Configure(EntityTypeBuilder<OrdenPreparacion> builder)
    {
        builder.ToTable("ordenes_preparacion");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();

        builder.Property(x => x.SucursalId)
               .HasColumnName("sucursal_id")
               .IsRequired();

        builder.Property(x => x.ComprobanteOrigenId)
               .HasColumnName("comprobante_origen_id");

        builder.Property(x => x.TerceroId)
               .HasColumnName("tercero_id");

        builder.Property(x => x.Fecha)
               .HasColumnName("fecha")
               .IsRequired();

        builder.Property(x => x.Estado)
               .HasColumnName("estado")
               .HasConversion(
                   v => v.ToString().ToUpperInvariant(),
                   v => Enum.Parse<EstadoOrdenPreparacion>(v, true))
               .HasMaxLength(20)
               .IsRequired();

        builder.Property(x => x.Observacion)
               .HasColumnName("observacion");

        builder.Property(x => x.FechaConfirmacion)
               .HasColumnName("fecha_confirmacion");

        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");

        builder.HasIndex(x => x.SucursalId);
        builder.HasIndex(x => x.Fecha);
    }
}

public class OrdenPreparacionDetalleConfiguration : IEntityTypeConfiguration<OrdenPreparacionDetalle>
{
    public void Configure(EntityTypeBuilder<OrdenPreparacionDetalle> builder)
    {
        builder.ToTable("ordenes_preparacion_detalles");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();

        builder.Property(x => x.OrdenPreparacionId)
               .HasColumnName("orden_preparacion_id")
               .IsRequired();

        builder.Property(x => x.ItemId)
               .HasColumnName("item_id")
               .IsRequired();

        builder.Property(x => x.DepositoId)
               .HasColumnName("deposito_id")
               .IsRequired();

        builder.Property(x => x.Cantidad)
               .HasColumnName("cantidad")
               .HasPrecision(18, 4)
               .IsRequired();

        builder.Property(x => x.CantidadEntregada)
               .HasColumnName("cantidad_entregada")
               .HasPrecision(18, 4)
               .IsRequired();

        builder.Property(x => x.Observacion)
               .HasColumnName("observacion");

        builder.HasIndex(x => x.OrdenPreparacionId);
    }
}
