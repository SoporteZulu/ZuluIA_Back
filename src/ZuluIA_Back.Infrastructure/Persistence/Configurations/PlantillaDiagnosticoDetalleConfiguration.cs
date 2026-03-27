using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZuluIA_Back.Domain.Entities.Configuracion;

namespace ZuluIA_Back.Infrastructure.Persistence.Configurations;

public class PlantillaDiagnosticoDetalleConfiguration : IEntityTypeConfiguration<PlantillaDiagnosticoDetalle>
{
    public void Configure(EntityTypeBuilder<PlantillaDiagnosticoDetalle> builder)
    {
        builder.ToTable("FRA_PLANTILLASDETALLE");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("plantd_id").UseIdentityColumn();

        builder.Property(x => x.PlantillaId)
               .HasColumnName("plant_id").IsRequired();

        builder.Property(x => x.VariableDetalleId)
               .HasColumnName("vard_id");

        builder.Property(x => x.PorcentajeIncidencia)
               .HasColumnName("plantd_porcentajeIncidencia").HasPrecision(10, 4).IsRequired();

        builder.Property(x => x.ValorObjetivo)
               .HasColumnName("plantd_valorObjetivo").HasPrecision(18, 4);

        builder.HasIndex(x => x.PlantillaId);
    }
}
