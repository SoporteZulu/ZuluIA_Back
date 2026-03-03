using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZuluIA_Back.Domain.Entities.Usuarios;

namespace ZuluIA_Back.Infrastructure.Persistence.Configurations;

public class SeguridadConfiguration : IEntityTypeConfiguration<Seguridad>
{
    public void Configure(EntityTypeBuilder<Seguridad> builder)
    {
        builder.ToTable("seguridad");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();

        builder.Property(x => x.Identificador)
               .HasColumnName("identificador").HasMaxLength(100).IsRequired();

        builder.Property(x => x.Descripcion)
               .HasColumnName("descripcion").HasMaxLength(300);

        builder.Property(x => x.AplicaSeguridadPorUsuario)
               .HasColumnName("aplica_seguridad_por_usuario").HasDefaultValue(true);

        builder.HasIndex(x => x.Identificador).IsUnique();
    }
}

public class SeguridadUsuarioConfiguration : IEntityTypeConfiguration<SeguridadUsuario>
{
    public void Configure(EntityTypeBuilder<SeguridadUsuario> builder)
    {
        builder.ToTable("seguridad_usuario");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();

        builder.Property(x => x.SeguridadId)
               .HasColumnName("seguridad_id").IsRequired();

        builder.Property(x => x.UsuarioId)
               .HasColumnName("usuario_id").IsRequired();

        builder.Property(x => x.Valor)
               .HasColumnName("valor").HasDefaultValue(false);

        builder.HasIndex(x => new { x.SeguridadId, x.UsuarioId }).IsUnique();
        builder.HasIndex(x => x.UsuarioId);
    }
}