using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZuluIA_Back.Domain.Entities.Franquicias;

namespace ZuluIA_Back.Infrastructure.Persistence.Configurations;

public class PlanTrabajoConfiguration : IEntityTypeConfiguration<PlanTrabajo>
{
    public void Configure(EntityTypeBuilder<PlanTrabajo> builder)
    {
        builder.ToTable("planes_trabajo");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).HasColumnName("id").UseIdentityColumn();
        builder.Property(p => p.Nombre).HasColumnName("nombre").HasMaxLength(200).IsRequired();
        builder.Property(p => p.SucursalId).HasColumnName("sucursal_id").IsRequired();
        builder.Property(p => p.Periodo).HasColumnName("periodo").IsRequired();
        builder.Property(p => p.FechaInicio).HasColumnName("fecha_inicio").IsRequired();
        builder.Property(p => p.FechaFin).HasColumnName("fecha_fin").IsRequired();
        builder.Property(p => p.Estado).HasColumnName("estado")
            .HasConversion(v => v.ToString().ToUpperInvariant(), v => Enum.Parse<Domain.Enums.EstadoPlanTrabajo>(v, true))
            .HasMaxLength(20).IsRequired();
        builder.Property(p => p.Descripcion).HasColumnName("descripcion").HasMaxLength(1000);
        builder.Property(p => p.CreatedAt).HasColumnName("created_at");
        builder.Property(p => p.UpdatedAt).HasColumnName("updated_at");
        builder.Property(p => p.DeletedAt).HasColumnName("deleted_at");
        builder.Property(p => p.CreatedBy).HasColumnName("created_by");
        builder.Property(p => p.UpdatedBy).HasColumnName("updated_by");

        builder.HasMany(p => p.Kpis).WithOne()
            .HasForeignKey(k => k.PlanTrabajoId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class PlanTrabajoKpiConfiguration : IEntityTypeConfiguration<PlanTrabajoKpi>
{
    public void Configure(EntityTypeBuilder<PlanTrabajoKpi> builder)
    {
        builder.ToTable("planes_trabajo_kpis");
        builder.HasKey(k => k.Id);
        builder.Property(k => k.Id).HasColumnName("id").UseIdentityColumn();
        builder.Property(k => k.PlanTrabajoId).HasColumnName("plan_trabajo_id").IsRequired();
        builder.Property(k => k.AspectoId).HasColumnName("aspecto_id");
        builder.Property(k => k.VariableId).HasColumnName("variable_id");
        builder.Property(k => k.Descripcion).HasColumnName("descripcion").HasMaxLength(500).IsRequired();
        builder.Property(k => k.ValorObjetivo).HasColumnName("valor_objetivo").HasColumnType("numeric(18,4)").IsRequired();
        builder.Property(k => k.Peso).HasColumnName("peso").HasColumnType("numeric(5,2)").IsRequired();
    }
}

public class EvaluacionFranquiciaConfiguration : IEntityTypeConfiguration<EvaluacionFranquicia>
{
    public void Configure(EntityTypeBuilder<EvaluacionFranquicia> builder)
    {
        builder.ToTable("evaluaciones_franquicias");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id").UseIdentityColumn();
        builder.Property(e => e.PlanTrabajoId).HasColumnName("plan_trabajo_id").IsRequired();
        builder.Property(e => e.SucursalId).HasColumnName("sucursal_id").IsRequired();
        builder.Property(e => e.Periodo).HasColumnName("periodo").IsRequired();
        builder.Property(e => e.PuntajeTotal).HasColumnName("puntaje_total").HasColumnType("numeric(8,2)").IsRequired();
        builder.Property(e => e.FechaEvaluacion).HasColumnName("fecha_evaluacion").IsRequired();
        builder.Property(e => e.Observacion).HasColumnName("observacion").HasMaxLength(1000);
        builder.Property(e => e.CreatedAt).HasColumnName("created_at");
        builder.Property(e => e.UpdatedAt).HasColumnName("updated_at");
        builder.Property(e => e.DeletedAt).HasColumnName("deleted_at");
        builder.Property(e => e.CreatedBy).HasColumnName("created_by");
        builder.Property(e => e.UpdatedBy).HasColumnName("updated_by");

        builder.HasMany(e => e.Detalles).WithOne()
            .HasForeignKey(d => d.EvaluacionId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class EvaluacionFranquiciaDetalleConfiguration : IEntityTypeConfiguration<EvaluacionFranquiciaDetalle>
{
    public void Configure(EntityTypeBuilder<EvaluacionFranquiciaDetalle> builder)
    {
        builder.ToTable("evaluaciones_franquicias_detalles");
        builder.HasKey(d => d.Id);
        builder.Property(d => d.Id).HasColumnName("id").UseIdentityColumn();
        builder.Property(d => d.EvaluacionId).HasColumnName("evaluacion_id").IsRequired();
        builder.Property(d => d.KpiId).HasColumnName("kpi_id").IsRequired();
        builder.Property(d => d.ValorReal).HasColumnName("valor_real").HasColumnType("numeric(18,4)").IsRequired();
        builder.Property(d => d.Puntaje).HasColumnName("puntaje").HasColumnType("numeric(8,2)").IsRequired();
        builder.Property(d => d.Observacion).HasColumnName("observacion").HasMaxLength(500);
    }
}
