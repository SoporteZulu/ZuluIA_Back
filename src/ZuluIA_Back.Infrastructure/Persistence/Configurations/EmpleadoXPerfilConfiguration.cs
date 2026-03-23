using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZuluIA_Back.Domain.Entities.RRHH;

namespace ZuluIA_Back.Infrastructure.Persistence.Configurations;

public class EmpleadoXPerfilConfiguration : IEntityTypeConfiguration<EmpleadoXPerfil>
{
    public void Configure(EntityTypeBuilder<EmpleadoXPerfil> builder)
    {
        builder.ToTable("empleado_area_perfil");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();

        builder.Property(x => x.EmpleadoXAreaId)
               .HasColumnName("empleado_area_id").IsRequired();

        builder.Property(x => x.PerfilId)
               .HasColumnName("perfil_id").IsRequired();

        builder.Property(x => x.Orden)
               .HasColumnName("orden").IsRequired();
    }
}
