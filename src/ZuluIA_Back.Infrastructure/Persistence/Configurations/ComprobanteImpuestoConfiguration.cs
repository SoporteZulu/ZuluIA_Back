using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZuluIA_Back.Domain.Entities.Comprobantes;

namespace ZuluIA_Back.Infrastructure.Persistence.Configurations;

public class ComprobanteImpuestoConfiguration : IEntityTypeConfiguration<ComprobanteImpuesto>
{
    public void Configure(EntityTypeBuilder<ComprobanteImpuesto> builder)
    {
        builder.ToTable("comprobantes_impuestos");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedOnAdd();

        builder.Property(x => x.ComprobanteId)
               .HasColumnName("comprobante_id").IsRequired();

        builder.Property(x => x.AlicuotaIvaId)
               .HasColumnName("alicuota_iva_id").IsRequired();

        builder.Property(x => x.PorcentajeIva)
               .HasColumnName("porcentaje_iva")
               .HasPrecision(10, 4)
               .HasDefaultValue(0m);

        builder.Property(x => x.BaseImponible)
               .HasColumnName("base_imponible")
               .HasPrecision(18, 4)
               .HasDefaultValue(0m);

        builder.Property(x => x.ImporteIva)
               .HasColumnName("importe_iva")
               .HasPrecision(18, 4)
               .HasDefaultValue(0m);

        builder.HasOne<Comprobante>()
               .WithMany()
               .HasForeignKey(x => x.ComprobanteId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
