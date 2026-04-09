using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZuluIA_Back.Domain.Entities.CRM;

namespace ZuluIA_Back.Infrastructure.Persistence.Configurations;

public class CrmUsuarioPerfilConfiguration : IEntityTypeConfiguration<CrmUsuarioPerfil>
{
    public void Configure(EntityTypeBuilder<CrmUsuarioPerfil> builder)
    {
        builder.ToTable("CRMUSUARIOS");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();

        builder.Property(x => x.UsuarioId).HasColumnName("usuario_id").IsRequired();
        builder.Property(x => x.Rol).HasColumnName("rol").HasMaxLength(30).IsRequired();
        builder.Property(x => x.Avatar).HasColumnName("avatar").HasMaxLength(500);
        builder.Property(x => x.Activo).HasColumnName("activo").HasDefaultValue(true);
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");
        builder.Property(x => x.CreatedBy).HasColumnName("created_by");
        builder.Property(x => x.UpdatedBy).HasColumnName("updated_by");

        builder.HasIndex(x => x.UsuarioId).IsUnique();
        builder.HasIndex(x => new { x.Rol, x.Activo });
    }
}
