using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZuluIA_Back.Domain.Entities.Usuarios;

namespace ZuluIA_Back.Infrastructure.Persistence.Configurations;

public class MenuUsuarioConfiguration : IEntityTypeConfiguration<MenuUsuario>
{
    public void Configure(EntityTypeBuilder<MenuUsuario> builder)
    {
        builder.ToTable("menu_usuario");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();

        builder.Property(x => x.MenuId)
               .HasColumnName("menu_id").IsRequired();

        builder.Property(x => x.UsuarioId)
               .HasColumnName("usuario_id").IsRequired();

        builder.HasIndex(x => new { x.MenuId, x.UsuarioId }).IsUnique();
        builder.HasIndex(x => x.UsuarioId);
    }
}