using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZuluIA_Back.Domain.Entities.Configuracion;

namespace ZuluIA_Back.Infrastructure.Persistence.Configurations;

public class PlanillaDiagnosticoDetalleConfiguration : IEntityTypeConfiguration<PlanillaDiagnosticoDetalle>
{
    public void Configure(EntityTypeBuilder<PlanillaDiagnosticoDetalle> builder)
    {
        builder.ToTable("FRA_PLANILLASDETALLE");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("pland_id").UseIdentityColumn();

        builder.Property(x => x.PlanillaId)
               .HasColumnName("plan_id").IsRequired();

        builder.Property(x => x.VariableDetalleId)
               .HasColumnName("vard_id");

        builder.Property(x => x.OpcionVariableId)
               .HasColumnName("opc_id");

        builder.Property(x => x.PuntajeVariable)
               .HasColumnName("pland_puntajeVariable").HasPrecision(10, 4).IsRequired();

        builder.Property(x => x.Valor)
               .HasColumnName("pland_valor").HasPrecision(18, 4).IsRequired();

        builder.Property(x => x.PorcentajeIncidencia)
               .HasColumnName("pland_porcentajeIncidencia").HasPrecision(10, 4).IsRequired();

        builder.Property(x => x.ValorObjetivo)
               .HasColumnName("pland_valorObjetivo").HasPrecision(18, 4);

        builder.HasIndex(x => x.PlanillaId);
    }
}
