using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZuluIA_Back.Domain.Entities.Usuarios;

namespace ZuluIA_Back.Infrastructure.Persistence.Configurations;

public class UsuarioSucursalConfiguration : IEntityTypeConfiguration<UsuarioSucursal>
{
    public void Configure(EntityTypeBuilder<UsuarioSucursal> builder)
    {
        builder.ToTable("usuarios_sucursal");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();

        builder.Property(x => x.UsuarioId)
               .HasColumnName("usuario_id").IsRequired();

        builder.Property(x => x.SucursalId)
               .HasColumnName("sucursal_id").IsRequired();

        builder.HasIndex(x => new { x.UsuarioId, x.SucursalId }).IsUnique();
    }
}