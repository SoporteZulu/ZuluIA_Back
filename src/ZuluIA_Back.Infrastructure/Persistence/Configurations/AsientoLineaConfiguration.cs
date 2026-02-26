using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZuluIA_Back.Domain.Entities.Contabilidad;

namespace ZuluIA_Back.Infrastructure.Persistence.Configurations;

public class AsientoLineaConfiguration : IEntityTypeConfiguration<AsientoLinea>
{
    public void Configure(EntityTypeBuilder<AsientoLinea> builder)
    {
        builder.ToTable("asientos_lineas");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();
        builder.Property(x => x.AsientoId).HasColumnName("asiento_id").IsRequired();
        builder.Property(x => x.CuentaId).HasColumnName("cuenta_id").IsRequired();
        builder.Property(x => x.Debe).HasColumnName("debe").HasPrecision(18, 2);
        builder.Property(x => x.Haber).HasColumnName("haber").HasPrecision(18, 2);
        builder.Property(x => x.Descripcion).HasColumnName("descripcion").HasMaxLength(300);
        builder.Property(x => x.Orden).HasColumnName("orden");
        builder.Property(x => x.CentroCostoId).HasColumnName("centro_costo_id");

        builder.HasIndex(x => x.AsientoId);
        builder.HasIndex(x => x.CuentaId);
    }
}