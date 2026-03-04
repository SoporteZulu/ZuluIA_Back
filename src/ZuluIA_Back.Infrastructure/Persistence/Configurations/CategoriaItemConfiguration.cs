using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZuluIA_Back.Domain.Entities.Items;

namespace ZuluIA_Back.Infrastructure.Persistence.Configurations;

public class CategoriaItemConfiguration : IEntityTypeConfiguration<CategoriaItem>
{
    public void Configure(EntityTypeBuilder<CategoriaItem> builder)
    {
        builder.ToTable("categorias_items");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();

        builder.Property(x => x.ParentId)
               .HasColumnName("parent_id");

        builder.Property(x => x.Codigo)
               .HasColumnName("codigo")
               .HasMaxLength(50)
               .IsRequired();

        builder.Property(x => x.Descripcion)
               .HasColumnName("descripcion")
               .HasMaxLength(200)
               .IsRequired();

        builder.Property(x => x.Nivel)
               .HasColumnName("nivel")
               .HasDefaultValue((short)1);

        builder.Property(x => x.OrdenNivel)
               .HasColumnName("orden_nivel")
               .HasMaxLength(50);

        builder.Property(x => x.Activo)
               .HasColumnName("activo")
               .HasDefaultValue(true);

        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");
        builder.Property(x => x.CreatedBy).HasColumnName("created_by");
        builder.Property(x => x.UpdatedBy).HasColumnName("updated_by");

        // Auto-referencia padre-hijo
        builder.HasMany(x => x.Hijos)
               .WithOne()
               .HasForeignKey(x => x.ParentId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.Codigo).IsUnique();
        builder.HasIndex(x => x.ParentId);
        builder.HasIndex(x => x.Activo);
    }
}