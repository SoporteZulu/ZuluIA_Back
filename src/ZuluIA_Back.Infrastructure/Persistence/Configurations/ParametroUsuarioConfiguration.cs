using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZuluIA_Back.Domain.Entities.Usuarios;

namespace ZuluIA_Back.Infrastructure.Persistence.Configurations;

public class ParametroUsuarioConfiguration : IEntityTypeConfiguration<ParametroUsuario>
{
    public void Configure(EntityTypeBuilder<ParametroUsuario> builder)
    {
        builder.ToTable("parametros_usuario");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();

        builder.Property(x => x.UsuarioId)
               .HasColumnName("usuario_id").IsRequired();

        builder.Property(x => x.Clave)
               .HasColumnName("clave").HasMaxLength(100).IsRequired();

        builder.Property(x => x.Valor)
               .HasColumnName("valor");

        builder.HasIndex(x => new { x.UsuarioId, x.Clave }).IsUnique();
    }
}