using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZuluIA_Back.Domain.Entities.Usuarios;

namespace ZuluIA_Back.Infrastructure.Persistence.Configurations;

public class MenuItemConfiguration : IEntityTypeConfiguration<MenuItem>
{
    public void Configure(EntityTypeBuilder<MenuItem> builder)
    {
        builder.ToTable("menu");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();

        builder.Property(x => x.ParentId)
               .HasColumnName("parent_id");

        builder.Property(x => x.Descripcion)
               .HasColumnName("descripcion").HasMaxLength(100).IsRequired();

        builder.Property(x => x.Formulario)
               .HasColumnName("formulario").HasMaxLength(100);

        builder.Property(x => x.Icono)
               .HasColumnName("icono").HasMaxLength(50);

        builder.Property(x => x.Nivel)
               .HasColumnName("nivel").HasDefaultValue((short)1);

        builder.Property(x => x.Orden)
               .HasColumnName("orden").HasDefaultValue((short)0);

        builder.Property(x => x.Activo)
               .HasColumnName("activo").HasDefaultValue(true);

        builder.Property(x => x.CreatedAt)
               .HasColumnName("created_at");

        // Auto-referencia padre-hijo
        builder.HasMany(x => x.Hijos)
               .WithOne()
               .HasForeignKey(x => x.ParentId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.ParentId);
        builder.HasIndex(x => x.Activo);
    }
}