using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZuluIA_Back.Domain.Entities.Finanzas;

namespace ZuluIA_Back.Infrastructure.Persistence.Configurations;

public class RetencionConfiguration : IEntityTypeConfiguration<Retencion>
{
    public void Configure(EntityTypeBuilder<Retencion> builder)
    {
        builder.ToTable("retenciones");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();

        builder.Property(x => x.PagoId)
               .HasColumnName("pago_id");

        builder.Property(x => x.CobroId)
               .HasColumnName("cobro_id");

        builder.Property(x => x.Tipo)
               .HasColumnName("tipo")
               .HasMaxLength(50).IsRequired();

        builder.Property(x => x.Importe)
               .HasColumnName("importe")
               .HasPrecision(18, 4).IsRequired();

        builder.Property(x => x.NroCertificado)
               .HasColumnName("nro_certificado")
               .HasMaxLength(50);

        builder.Property(x => x.Fecha)
               .HasColumnName("fecha").IsRequired();

        builder.Property(x => x.CreatedAt)
               .HasColumnName("created_at");

        builder.HasIndex(x => x.PagoId);
        builder.HasIndex(x => x.CobroId);
    }
}