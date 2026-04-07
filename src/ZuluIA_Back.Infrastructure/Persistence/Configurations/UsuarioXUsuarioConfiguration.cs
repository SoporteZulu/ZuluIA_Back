using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZuluIA_Back.Domain.Entities.Usuarios;

namespace ZuluIA_Back.Infrastructure.Persistence.Configurations;

public class UsuarioXUsuarioConfiguration : IEntityTypeConfiguration<UsuarioXUsuario>
{
    public void Configure(EntityTypeBuilder<UsuarioXUsuario> builder)
    {
        builder.ToTable("SEG_USUARIOXUSUARIO");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("uxu_id").UseIdentityColumn();

        builder.Property(x => x.UsuarioMiembroId)
               .HasColumnName("usr_id_miembro").IsRequired();

        builder.Property(x => x.UsuarioGrupoId)
               .HasColumnName("usr_id_grupo").IsRequired();

        builder.HasIndex(x => new { x.UsuarioMiembroId, x.UsuarioGrupoId }).IsUnique();
    }
}
