using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZuluIA_Back.Domain.Entities.Geografia;

namespace ZuluIA_Back.Infrastructure.Persistence.Configurations;

public class RegionConfiguration : IEntityTypeConfiguration<Region>
{
    public void Configure(EntityTypeBuilder<Region> builder)
    {
        builder.ToTable("region");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();

        builder.Property(x => x.Codigo)
               .HasColumnName("codigo").HasMaxLength(50).IsRequired();

        builder.Property(x => x.Descripcion)
               .HasColumnName("descripcion").HasMaxLength(200).IsRequired();

        builder.Property(x => x.RegionIntegradoraId)
               .HasColumnName("region_integradora_id");

        builder.Property(x => x.Orden)
               .HasColumnName("orden").IsRequired();

        builder.Property(x => x.Nivel)
               .HasColumnName("nivel").IsRequired();

        builder.Property(x => x.CodigoEstructura)
               .HasColumnName("codigo_estructura").HasMaxLength(200);

        builder.Property(x => x.EsRegionIntegradora)
               .HasColumnName("es_region_integradora").IsRequired();

        builder.Property(x => x.Observacion)
               .HasColumnName("observacion").HasMaxLength(500);

        builder.HasIndex(x => x.Codigo).IsUnique();
    }
}
