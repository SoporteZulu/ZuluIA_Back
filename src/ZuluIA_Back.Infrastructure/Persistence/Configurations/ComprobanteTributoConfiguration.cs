using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZuluIA_Back.Domain.Entities.Comprobantes;

namespace ZuluIA_Back.Infrastructure.Persistence.Configurations;

public class ComprobanteTributoConfiguration : IEntityTypeConfiguration<ComprobanteTributo>
{
    public void Configure(EntityTypeBuilder<ComprobanteTributo> builder)
    {
        builder.ToTable("comprobantes_tributos");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedOnAdd();

        builder.Property(x => x.ComprobanteId)
            .HasColumnName("comprobante_id")
            .IsRequired();

        builder.Property(x => x.ImpuestoId)
            .HasColumnName("impuesto_id");

        builder.Property(x => x.Codigo)
            .HasColumnName("codigo")
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.Descripcion)
            .HasColumnName("descripcion")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.BaseImponible)
            .HasColumnName("base_imponible")
            .HasPrecision(18, 4)
            .HasDefaultValue(0m);

        builder.Property(x => x.Alicuota)
            .HasColumnName("alicuota")
            .HasPrecision(10, 4)
            .HasDefaultValue(0m);

        builder.Property(x => x.Importe)
            .HasColumnName("importe")
            .HasPrecision(18, 4)
            .HasDefaultValue(0m);

        builder.Property(x => x.Orden)
            .HasColumnName("orden")
            .HasDefaultValue(0);

        builder.HasOne<Comprobante>()
            .WithMany()
            .HasForeignKey(x => x.ComprobanteId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}