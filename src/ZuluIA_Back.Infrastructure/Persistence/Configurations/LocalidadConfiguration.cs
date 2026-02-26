using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZuluIA_Back.Domain.Entities.Geografia;

namespace ZuluIA_Back.Infrastructure.Persistence.Configurations;

public class LocalidadConfiguration : IEntityTypeConfiguration<Localidad>
{
    public void Configure(EntityTypeBuilder<Localidad> builder)
    {
        builder.ToTable("localidades");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();
        builder.Property(x => x.ProvinciaId).HasColumnName("provincia_id").IsRequired();
        builder.Property(x => x.CodigoPostal).HasColumnName("codigo_postal").HasMaxLength(10);
        builder.Property(x => x.Descripcion).HasColumnName("descripcion").HasMaxLength(100).IsRequired();
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");

        builder.HasOne(x => x.Provincia)
               .WithMany()
               .HasForeignKey(x => x.ProvinciaId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}