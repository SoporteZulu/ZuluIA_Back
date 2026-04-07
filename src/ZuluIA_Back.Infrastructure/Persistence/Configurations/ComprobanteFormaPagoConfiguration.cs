using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZuluIA_Back.Domain.Entities.Comprobantes;

namespace ZuluIA_Back.Infrastructure.Persistence.Configurations;

public class ComprobanteFormaPagoConfiguration : IEntityTypeConfiguration<ComprobanteFormaPago>
{
    public void Configure(EntityTypeBuilder<ComprobanteFormaPago> builder)
    {
        builder.ToTable("comprobante_forma_pago");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();

        builder.Property(x => x.ComprobanteId)
               .HasColumnName("comprobante_id").IsRequired();

        builder.Property(x => x.FormaPagoId)
               .HasColumnName("forma_pago_id").IsRequired();

        builder.Property(x => x.Fecha)
               .HasColumnName("fecha").IsRequired();

        builder.Property(x => x.Importe)
               .HasColumnName("importe").HasColumnType("numeric(18,4)").IsRequired();

        builder.Property(x => x.Descripcion)
               .HasColumnName("descripcion").HasMaxLength(200);

        builder.Property(x => x.Observacion)
               .HasColumnName("observacion").HasMaxLength(500);

        builder.Property(x => x.Valido)
               .HasColumnName("valido").IsRequired();

        builder.Property(x => x.MonedaId)
               .HasColumnName("moneda_id");

        builder.Property(x => x.Cotizacion)
               .HasColumnName("cotizacion").HasColumnType("numeric(18,6)").IsRequired();

        builder.HasIndex(x => x.ComprobanteId);
    }
}
