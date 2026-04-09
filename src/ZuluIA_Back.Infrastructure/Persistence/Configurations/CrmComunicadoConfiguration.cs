using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZuluIA_Back.Domain.Entities.CRM;

namespace ZuluIA_Back.Infrastructure.Persistence.Configurations;

public class CrmComunicadoConfiguration : IEntityTypeConfiguration<CrmComunicado>
{
    public void Configure(EntityTypeBuilder<CrmComunicado> builder)
    {
        builder.ToTable("CRMCOMUNICADOS");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();

        builder.Property(x => x.SucursalId).HasColumnName("sucursal_id").IsRequired();
        builder.Property(x => x.TerceroId).HasColumnName("tercero_id").IsRequired();
        builder.Property(x => x.CampanaId).HasColumnName("campana_id");
        builder.Property(x => x.TipoId).HasColumnName("tipo_id");
        builder.Property(x => x.Fecha).HasColumnName("fecha").IsRequired();
        builder.Property(x => x.Asunto).HasColumnName("asunto").HasMaxLength(300).IsRequired();
        builder.Property(x => x.Contenido).HasColumnName("contenido");
        builder.Property(x => x.UsuarioId).HasColumnName("usuario_id");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");
        builder.Property(x => x.CreatedBy).HasColumnName("created_by");
        builder.Property(x => x.UpdatedBy).HasColumnName("updated_by");

        builder.HasIndex(x => new { x.SucursalId, x.Fecha });
        builder.HasIndex(x => x.TerceroId);
        builder.HasIndex(x => x.CampanaId);
    }
}
