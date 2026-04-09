using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZuluIA_Back.Domain.Entities.CRM;

namespace ZuluIA_Back.Infrastructure.Persistence.Configurations;

public class CrmInteraccionConfiguration : IEntityTypeConfiguration<CrmInteraccion>
{
    public void Configure(EntityTypeBuilder<CrmInteraccion> builder)
    {
        builder.ToTable("CRMINTERACCIONES");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();

        builder.Property(x => x.ClienteId).HasColumnName("cliente_id").IsRequired();
        builder.Property(x => x.ContactoId).HasColumnName("contacto_id");
        builder.Property(x => x.OportunidadId).HasColumnName("oportunidad_id");
        builder.Property(x => x.TipoInteraccion).HasColumnName("tipo_interaccion").HasMaxLength(40).IsRequired();
        builder.Property(x => x.Canal).HasColumnName("canal").HasMaxLength(40).IsRequired();
        builder.Property(x => x.FechaHora).HasColumnName("fecha_hora").IsRequired();
        builder.Property(x => x.UsuarioResponsableId).HasColumnName("usuario_responsable_id").IsRequired();
        builder.Property(x => x.Resultado).HasColumnName("resultado").HasMaxLength(40).IsRequired();
        builder.Property(x => x.Descripcion).HasColumnName("descripcion").HasMaxLength(2000);
        builder.Property(x => x.AdjuntosJson).HasColumnName("adjuntos_json").HasColumnType("jsonb").HasDefaultValue("[]");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");
        builder.Property(x => x.CreatedBy).HasColumnName("created_by");
        builder.Property(x => x.UpdatedBy).HasColumnName("updated_by");

        builder.HasIndex(x => new { x.ClienteId, x.FechaHora });
        builder.HasIndex(x => x.OportunidadId);
    }
}
