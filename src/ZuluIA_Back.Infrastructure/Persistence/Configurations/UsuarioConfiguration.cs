using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZuluIA_Back.Domain.Entities.Usuarios;

namespace ZuluIA_Back.Infrastructure.Persistence.Configurations;

public class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.ToTable("usuarios");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();

        builder.Property(x => x.SupabaseUserId)
               .HasColumnName("supabase_user_id");

        builder.Property(x => x.UserName)
               .HasColumnName("usuario")
               .HasMaxLength(100)
               .IsRequired();

        builder.Property(x => x.NombreCompleto)
               .HasColumnName("nombre_completo")
               .HasMaxLength(200);

        builder.Property(x => x.Email)
               .HasColumnName("email")
               .HasMaxLength(150);

        builder.Property(x => x.Activo)
               .HasColumnName("activo")
               .HasDefaultValue(true);

        builder.Property(x => x.ArqueoActual)
               .HasColumnName("arqueo_actual");

        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");

        // Ignorar propiedades de AuditableEntity no usadas en usuarios
        builder.Ignore(x => x.CreatedBy);
        builder.Ignore(x => x.UpdatedBy);

        builder.HasMany(x => x.Sucursales)
               .WithOne()
               .HasForeignKey(x => x.UsuarioId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.UserName).IsUnique();
        builder.HasIndex(x => x.SupabaseUserId).IsUnique();
        builder.HasIndex(x => x.Activo);
    }
}