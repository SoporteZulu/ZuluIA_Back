using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZuluIA_Back.Domain.Entities.CRM;

namespace ZuluIA_Back.Infrastructure.Persistence.Configurations;

public class CrmCampanaConfiguration : IEntityTypeConfiguration<CrmCampana>
{
    public void Configure(EntityTypeBuilder<CrmCampana> builder)
    {
        builder.ToTable("CRMCAMPANAS");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();

        builder.Property(x => x.SucursalId).HasColumnName("sucursal_id").IsRequired();
        builder.Property(x => x.Nombre).HasColumnName("nombre").HasMaxLength(200).IsRequired();
        builder.Property(x => x.Descripcion).HasColumnName("descripcion").HasMaxLength(1000);
        builder.Property(x => x.TipoCampana).HasColumnName("tipo_campana").HasMaxLength(30).HasDefaultValue("email");
        builder.Property(x => x.Objetivo).HasColumnName("objetivo").HasMaxLength(40).HasDefaultValue("generacion_leads");
        builder.Property(x => x.SegmentoObjetivoId).HasColumnName("segmento_objetivo_id");
        builder.Property(x => x.FechaInicio).HasColumnName("fecha_inicio").IsRequired();
        builder.Property(x => x.FechaFin).HasColumnName("fecha_fin").IsRequired();
        builder.Property(x => x.Presupuesto).HasColumnName("presupuesto").HasPrecision(18, 2);
        builder.Property(x => x.PresupuestoGastado).HasColumnName("presupuesto_gastado").HasPrecision(18, 2);
        builder.Property(x => x.ResponsableId).HasColumnName("responsable_id");
        builder.Property(x => x.Notas).HasColumnName("notas").HasMaxLength(2000);
        builder.Property(x => x.LeadsGenerados).HasColumnName("leads_generados").HasDefaultValue(0);
        builder.Property(x => x.OportunidadesGeneradas).HasColumnName("oportunidades_generadas").HasDefaultValue(0);
        builder.Property(x => x.NegociosGanados).HasColumnName("negocios_ganados").HasDefaultValue(0);
        builder.Property(x => x.Activa).HasColumnName("activa").HasDefaultValue(true);
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");
        builder.Property(x => x.CreatedBy).HasColumnName("created_by");
        builder.Property(x => x.UpdatedBy).HasColumnName("updated_by");

        builder.HasIndex(x => new { x.SucursalId, x.FechaInicio });
        builder.HasIndex(x => x.SegmentoObjetivoId);
    }
}
