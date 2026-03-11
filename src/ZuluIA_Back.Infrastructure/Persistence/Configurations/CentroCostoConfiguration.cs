using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZuluIA_Back.Domain.Entities.Contabilidad;

namespace ZuluIA_Back.Infrastructure.Persistence.Configurations;

public class CentroCostoConfiguration : IEntityTypeConfiguration<CentroCosto>
{
    public void Configure(EntityTypeBuilder<CentroCosto> builder)
    {
        builder.ToTable("centros_costo");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();

        builder.Property(x => x.Codigo)
               .HasColumnName("codigo")
               .HasMaxLength(20)
               .IsRequired();

        builder.Property(x => x.Descripcion)
               .HasColumnName("descripcion")
               .HasMaxLength(200)
               .IsRequired();

        builder.Property(x => x.Activo)
               .HasColumnName("activo")
               .HasDefaultValue(true);

        builder.Property(x => x.CreatedAt)
               .HasColumnName("created_at");

        builder.HasIndex(x => x.Codigo).IsUnique();
        builder.HasIndex(x => x.Activo);
    }
}