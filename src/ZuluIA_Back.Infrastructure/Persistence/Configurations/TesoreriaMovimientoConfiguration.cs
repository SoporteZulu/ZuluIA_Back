using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZuluIA_Back.Domain.Entities.Finanzas;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Infrastructure.Persistence.Configurations;

public class TesoreriaMovimientoConfiguration : IEntityTypeConfiguration<TesoreriaMovimiento>
{
    public void Configure(EntityTypeBuilder<TesoreriaMovimiento> builder)
    {
        builder.ToTable("tesoreria_movimientos");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();
        builder.Property(x => x.SucursalId).HasColumnName("sucursal_id").IsRequired();
        builder.Property(x => x.CajaCuentaId).HasColumnName("caja_cuenta_id").IsRequired();
        builder.Property(x => x.Fecha).HasColumnName("fecha").IsRequired();
        builder.Property(x => x.TipoOperacion)
            .HasColumnName("tipo_operacion")
            .HasConversion(v => v.ToString().ToUpperInvariant(), v => Enum.Parse<TipoOperacionTesoreria>(v, true))
            .HasMaxLength(40)
            .IsRequired();
        builder.Property(x => x.Sentido)
            .HasColumnName("sentido")
            .HasConversion(v => v.ToString().ToUpperInvariant(), v => Enum.Parse<SentidoMovimientoTesoreria>(v, true))
            .HasMaxLength(20)
            .IsRequired();
        builder.Property(x => x.TerceroId).HasColumnName("tercero_id");
        builder.Property(x => x.Importe).HasColumnName("importe").HasPrecision(18, 4).IsRequired();
        builder.Property(x => x.MonedaId).HasColumnName("moneda_id").IsRequired();
        builder.Property(x => x.Cotizacion).HasColumnName("cotizacion").HasPrecision(18, 6).HasDefaultValue(1m);
        builder.Property(x => x.ReferenciaTipo).HasColumnName("referencia_tipo").HasMaxLength(40);
        builder.Property(x => x.ReferenciaId).HasColumnName("referencia_id");
        builder.Property(x => x.Observacion).HasColumnName("observacion").HasMaxLength(500);
        builder.Property(x => x.Anulado).HasColumnName("anulado").HasDefaultValue(false).IsRequired();
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");
        builder.Property(x => x.CreatedBy).HasColumnName("created_by");
        builder.Property(x => x.UpdatedBy).HasColumnName("updated_by");
        builder.HasIndex(x => x.CajaCuentaId);
        builder.HasIndex(x => x.Fecha);
        builder.HasIndex(x => x.TipoOperacion);
        builder.HasIndex(x => x.ReferenciaTipo);
    }
}
