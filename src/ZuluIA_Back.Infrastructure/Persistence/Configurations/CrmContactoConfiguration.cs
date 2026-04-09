using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZuluIA_Back.Domain.Entities.CRM;

namespace ZuluIA_Back.Infrastructure.Persistence.Configurations;

public class CrmContactoConfiguration : IEntityTypeConfiguration<CrmContacto>
{
    public void Configure(EntityTypeBuilder<CrmContacto> builder)
    {
        builder.ToTable("CRMCONTACTOS");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();

        builder.Property(x => x.ClienteId).HasColumnName("cliente_id").IsRequired();
        builder.Property(x => x.Nombre).HasColumnName("nombre").HasMaxLength(120).IsRequired();
        builder.Property(x => x.Apellido).HasColumnName("apellido").HasMaxLength(120).IsRequired();
        builder.Property(x => x.Cargo).HasColumnName("cargo").HasMaxLength(120);
        builder.Property(x => x.Email).HasColumnName("email").HasMaxLength(200);
        builder.Property(x => x.Telefono).HasColumnName("telefono").HasMaxLength(60);
        builder.Property(x => x.CanalPreferido).HasColumnName("canal_preferido").HasMaxLength(30).IsRequired();
        builder.Property(x => x.EstadoContacto).HasColumnName("estado_contacto").HasMaxLength(30).IsRequired();
        builder.Property(x => x.Notas).HasColumnName("notas").HasMaxLength(2000);
        builder.Property(x => x.Activo).HasColumnName("activo").HasDefaultValue(true);
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");
        builder.Property(x => x.CreatedBy).HasColumnName("created_by");
        builder.Property(x => x.UpdatedBy).HasColumnName("updated_by");

        builder.HasIndex(x => new { x.ClienteId, x.Activo });
    }
}
