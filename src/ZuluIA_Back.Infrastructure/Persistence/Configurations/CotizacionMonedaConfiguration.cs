using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZuluIA_Back.Domain.Entities.Finanzas;

namespace ZuluIA_Back.Infrastructure.Persistence.Configurations;

public class CotizacionMonedaConfiguration : IEntityTypeConfiguration<CotizacionMoneda>
{
    public void Configure(EntityTypeBuilder<CotizacionMoneda> builder)
    {
        builder.ToTable("cotizaciones_moneda");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();

        builder.Property(x => x.MonedaId)
               .HasColumnName("moneda_id").IsRequired();

        builder.Property(x => x.Fecha)
               .HasColumnName("fecha").IsRequired();

        builder.Property(x => x.Cotizacion)
               .HasColumnName("cotizacion").HasPrecision(18, 6).IsRequired();

        builder.Property(x => x.CreatedAt)
               .HasColumnName("created_at");

        builder.HasIndex(x => new { x.MonedaId, x.Fecha }).IsUnique();
    }
}