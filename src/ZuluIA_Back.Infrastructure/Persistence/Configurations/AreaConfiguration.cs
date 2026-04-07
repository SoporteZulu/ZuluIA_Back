using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZuluIA_Back.Domain.Entities.Sucursales;

namespace ZuluIA_Back.Infrastructure.Persistence.Configurations;

public class AreaConfiguration : IEntityTypeConfiguration<Area>
{
    public void Configure(EntityTypeBuilder<Area> builder)
    {
        builder.ToTable("area");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();

        builder.Property(x => x.Descripcion)
               .HasColumnName("descripcion").HasMaxLength(200).IsRequired();

        builder.Property(x => x.Codigo)
               .HasColumnName("codigo").HasMaxLength(50);

        builder.Property(x => x.SucursalId)
               .HasColumnName("sucursal_id");
    }
}
