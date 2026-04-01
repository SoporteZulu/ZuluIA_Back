using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZuluIA_Back.Domain.Entities.Terceros;

namespace ZuluIA_Back.Infrastructure.Persistence.Configurations;

public class EstadoPersonaCatalogoConfiguration : IEntityTypeConfiguration<EstadoPersonaCatalogo>
{
    public void Configure(EntityTypeBuilder<EstadoPersonaCatalogo> builder)
    {
        builder.ToTable("estados_personas");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();
        builder.Property(x => x.Descripcion).HasColumnName("descripcion").HasMaxLength(100).IsRequired();
        builder.Property(x => x.Activo).HasColumnName("activo").HasDefaultValue(true).IsRequired();
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");
        builder.Property(x => x.CreatedBy).HasColumnName("created_by");
        builder.Property(x => x.UpdatedBy).HasColumnName("updated_by");
        builder.HasIndex(x => x.Descripcion).IsUnique().HasDatabaseName("ux_estados_personas_descripcion");
    }
}
