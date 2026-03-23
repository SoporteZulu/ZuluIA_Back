using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZuluIA_Back.Domain.Entities.Configuracion;

namespace ZuluIA_Back.Infrastructure.Persistence.Configurations;

public class PlanillaDiagnosticoConfiguration : IEntityTypeConfiguration<PlanillaDiagnostico>
{
    public void Configure(EntityTypeBuilder<PlanillaDiagnostico> builder)
    {
        builder.ToTable("FRA_PLANILLAS");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("plan_id").UseIdentityColumn();

        builder.Property(x => x.ClienteId)
               .HasColumnName("cli_id");

        builder.Property(x => x.PlantillaId)
               .HasColumnName("plant_id");

        builder.Property(x => x.TipoPlanillaId)
               .HasColumnName("tplan_id");

        builder.Property(x => x.PlanillaPadreId)
               .HasColumnName("plan_idPlanillaPadre");

        builder.Property(x => x.EstadoId)
               .HasColumnName("estp_id");

        builder.Property(x => x.FechaEvaluacion)
               .HasColumnName("plan_fechaEvaluacion");

        builder.Property(x => x.FechaDesde)
               .HasColumnName("plan_fechaDesde");

        builder.Property(x => x.FechaHasta)
               .HasColumnName("plan_fechahasta");

        builder.Property(x => x.FechaRegistro)
               .HasColumnName("plan_fechaRegistro");

        builder.Property(x => x.Observaciones)
               .HasColumnName("plan_observaciones").HasMaxLength(1000);

        builder.HasIndex(x => x.ClienteId);
    }
}
