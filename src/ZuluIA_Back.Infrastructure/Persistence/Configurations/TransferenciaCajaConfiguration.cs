using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZuluIA_Back.Domain.Entities.Finanzas;

namespace ZuluIA_Back.Infrastructure.Persistence.Configurations;

public class TransferenciaCajaConfiguration : IEntityTypeConfiguration<TransferenciaCaja>
{
    public void Configure(EntityTypeBuilder<TransferenciaCaja> builder)
    {
        builder.ToTable("transferencias_caja");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();

        builder.Property(x => x.SucursalId)
               .HasColumnName("sucursal_id")
               .IsRequired();

        builder.Property(x => x.CajaOrigenId)
               .HasColumnName("caja_origen_id")
               .IsRequired();

        builder.Property(x => x.CajaDestinoId)
               .HasColumnName("caja_destino_id")
               .IsRequired();

        builder.Property(x => x.Fecha)
               .HasColumnName("fecha")
               .IsRequired();

        builder.Property(x => x.Importe)
               .HasColumnName("importe")
               .HasPrecision(18, 4)
               .IsRequired();

        builder.Property(x => x.MonedaId)
               .HasColumnName("moneda_id")
               .IsRequired();

        builder.Property(x => x.Cotizacion)
               .HasColumnName("cotizacion")
               .HasPrecision(18, 6)
               .HasDefaultValue(1m);

        builder.Property(x => x.Concepto)
               .HasColumnName("concepto")
               .HasMaxLength(500);

        builder.Property(x => x.MovimientoOrigenId)
               .HasColumnName("movimiento_origen_id");

        builder.Property(x => x.MovimientoDestinoId)
               .HasColumnName("movimiento_destino_id");

        builder.Property(x => x.Anulada)
               .HasColumnName("anulada")
               .HasDefaultValue(false);

        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");

        builder.HasIndex(x => x.SucursalId);
        builder.HasIndex(x => x.Fecha);
    }
}
