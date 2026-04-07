using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZuluIA_Back.Domain.Entities.Finanzas;

namespace ZuluIA_Back.Infrastructure.Persistence.Configurations;

public class EscalaRetencionConfiguration : IEntityTypeConfiguration<EscalaRetencion>
{
    public void Configure(EntityTypeBuilder<EscalaRetencion> builder)
    {
        builder.ToTable("escalas_retencion");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();

        builder.Property(x => x.TipoRetencionId)
               .HasColumnName("tipo_retencion_id")
               .IsRequired();

        builder.Property(x => x.Descripcion)
               .HasColumnName("descripcion")
               .HasMaxLength(200)
               .IsRequired();

        builder.Property(x => x.ImporteDesde)
               .HasColumnName("importe_desde")
               .HasPrecision(18, 4)
               .IsRequired();

        builder.Property(x => x.ImporteHasta)
               .HasColumnName("importe_hasta")
               .HasPrecision(18, 4)
               .IsRequired();

        builder.Property(x => x.Porcentaje)
               .HasColumnName("porcentaje")
               .HasPrecision(10, 4)
               .IsRequired();

        builder.HasIndex(x => x.TipoRetencionId);
    }
}
