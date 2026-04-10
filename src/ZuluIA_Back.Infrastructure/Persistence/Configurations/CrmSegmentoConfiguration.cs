using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZuluIA_Back.Domain.Entities.CRM;

namespace ZuluIA_Back.Infrastructure.Persistence.Configurations;

public class CrmSegmentoConfiguration : IEntityTypeConfiguration<CrmSegmento>
{
    public void Configure(EntityTypeBuilder<CrmSegmento> builder)
    {
        builder.ToTable("CRMSEGMENTOS");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();

        builder.Property(x => x.Nombre).HasColumnName("nombre").HasMaxLength(160).IsRequired();
        builder.Property(x => x.Descripcion).HasColumnName("descripcion").HasMaxLength(1000);
        builder.Property(x => x.CriteriosJson).HasColumnName("criterios_json").HasColumnType("jsonb").HasDefaultValue("[]");
        builder.Property(x => x.TipoSegmento).HasColumnName("tipo_segmento").HasMaxLength(20).IsRequired();
        builder.Property(x => x.Activo).HasColumnName("activo").HasDefaultValue(true);
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");
        builder.Property(x => x.CreatedBy).HasColumnName("created_by");
        builder.Property(x => x.UpdatedBy).HasColumnName("updated_by");

        builder.HasIndex(x => x.Nombre).IsUnique();
    }
}
