using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZuluIA_Back.Domain.Entities.Geografia;

namespace ZuluIA_Back.Infrastructure.Persistence.Configurations;

public class ProvinciaConfiguration : IEntityTypeConfiguration<Provincia>
{
    public void Configure(EntityTypeBuilder<Provincia> builder)
    {
        builder.ToTable("provincias");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();
        builder.Property(x => x.PaisId).HasColumnName("pais_id").IsRequired();
        builder.Property(x => x.Codigo).HasColumnName("codigo").HasMaxLength(10).IsRequired();
        builder.Property(x => x.Descripcion).HasColumnName("descripcion").HasMaxLength(100).IsRequired();
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");
        builder.Property(x => x.CreatedBy).HasColumnName("created_by");
        builder.Property(x => x.UpdatedBy).HasColumnName("updated_by");

        builder.HasOne(x => x.Pais)
               .WithMany()
               .HasForeignKey(x => x.PaisId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}