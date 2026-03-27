using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZuluIA_Back.Domain.Entities.RRHH;

namespace ZuluIA_Back.Infrastructure.Persistence.Configurations;

public class EmpleadoXAreaConfiguration : IEntityTypeConfiguration<EmpleadoXArea>
{
    public void Configure(EntityTypeBuilder<EmpleadoXArea> builder)
    {
        builder.ToTable("empleado_area");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();

        builder.Property(x => x.EmpleadoId)
               .HasColumnName("empleado_id").IsRequired();

        builder.Property(x => x.AreaId)
               .HasColumnName("area_id").IsRequired();

        builder.Property(x => x.Orden)
               .HasColumnName("orden").IsRequired();

        builder.HasIndex(x => new { x.EmpleadoId, x.AreaId }).IsUnique();
    }
}
