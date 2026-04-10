using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZuluIA_Back.Domain.Entities.CRM;

namespace ZuluIA_Back.Infrastructure.Persistence.Configurations;

public class CrmTareaConfiguration : IEntityTypeConfiguration<CrmTarea>
{
    public void Configure(EntityTypeBuilder<CrmTarea> builder)
    {
        builder.ToTable("CRMTAREAS");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();

        builder.Property(x => x.ClienteId).HasColumnName("cliente_id");
        builder.Property(x => x.OportunidadId).HasColumnName("oportunidad_id");
        builder.Property(x => x.AsignadoAId).HasColumnName("asignado_a_id").IsRequired();
        builder.Property(x => x.Titulo).HasColumnName("titulo").HasMaxLength(200).IsRequired();
        builder.Property(x => x.Descripcion).HasColumnName("descripcion").HasMaxLength(2000);
        builder.Property(x => x.TipoTarea).HasColumnName("tipo_tarea").HasMaxLength(40).IsRequired();
        builder.Property(x => x.FechaVencimiento).HasColumnName("fecha_vencimiento").IsRequired();
        builder.Property(x => x.Prioridad).HasColumnName("prioridad").HasMaxLength(20).IsRequired();
        builder.Property(x => x.Estado).HasColumnName("estado").HasMaxLength(20).IsRequired();
        builder.Property(x => x.FechaCompletado).HasColumnName("fecha_completado");
        builder.Property(x => x.Activa).HasColumnName("activa").HasDefaultValue(true);
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");
        builder.Property(x => x.CreatedBy).HasColumnName("created_by");
        builder.Property(x => x.UpdatedBy).HasColumnName("updated_by");

        builder.HasIndex(x => new { x.AsignadoAId, x.Estado, x.FechaVencimiento });
        builder.HasIndex(x => x.ClienteId);
        builder.HasIndex(x => x.OportunidadId);
    }
}
