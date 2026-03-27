using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZuluIA_Back.Domain.Entities.Configuracion;

namespace ZuluIA_Back.Infrastructure.Persistence.Configurations;

public class PlantillaDiagnosticoConfiguration : IEntityTypeConfiguration<PlantillaDiagnostico>
{
    public void Configure(EntityTypeBuilder<PlantillaDiagnostico> builder)
    {
        builder.ToTable("FRA_PLANTILLAS");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("plant_id").UseIdentityColumn();

        builder.Property(x => x.Descripcion)
               .HasColumnName("plant_descripcion").HasMaxLength(200).IsRequired();

        builder.Property(x => x.FechaDesde)
               .HasColumnName("plant_FechaDesde");

        builder.Property(x => x.FechaHasta)
               .HasColumnName("plant_fechahasta");

        builder.Property(x => x.FechaRegistro)
               .HasColumnName("plant_fechaRegistro");

        builder.Property(x => x.Observaciones)
               .HasColumnName("plant_observaciones").HasMaxLength(1000);
    }
}
