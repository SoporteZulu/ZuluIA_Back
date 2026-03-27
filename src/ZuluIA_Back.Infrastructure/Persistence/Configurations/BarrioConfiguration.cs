using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZuluIA_Back.Domain.Entities.Geografia;

namespace ZuluIA_Back.Infrastructure.Persistence.Configurations;

public class BarrioConfiguration : IEntityTypeConfiguration<Barrio>
{
    public void Configure(EntityTypeBuilder<Barrio> builder)
    {
        builder.ToTable("barrios");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();

        builder.Property(x => x.LocalidadId)
               .HasColumnName("localidad_id")
               .IsRequired();

        builder.Property(x => x.Descripcion)
               .HasColumnName("descripcion")
               .HasMaxLength(100)
               .IsRequired();

        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");

        builder.HasOne(x => x.Localidad)
               .WithMany()
               .HasForeignKey(x => x.LocalidadId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
