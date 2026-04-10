using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZuluIA_Back.Domain.Entities.CRM;

namespace ZuluIA_Back.Infrastructure.Persistence.Configurations;

public class CrmMotivoConfiguration : IEntityTypeConfiguration<CrmMotivo>
{
    public void Configure(EntityTypeBuilder<CrmMotivo> builder)
    {
        builder.ToTable("CRMMOTIVOS");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();

        builder.Property(x => x.Codigo).HasColumnName("codigo").HasMaxLength(30).IsRequired();
        builder.Property(x => x.Descripcion).HasColumnName("descripcion").HasMaxLength(200).IsRequired();
        builder.Property(x => x.Activo).HasColumnName("activo").HasDefaultValue(true);
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");
        builder.Property(x => x.CreatedBy).HasColumnName("created_by");
        builder.Property(x => x.UpdatedBy).HasColumnName("updated_by");

        builder.HasIndex(x => x.Codigo).IsUnique();
        builder.HasIndex(x => x.Activo);
    }
}
