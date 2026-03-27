using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZuluIA_Back.Domain.Entities.BI;

namespace ZuluIA_Back.Infrastructure.Persistence.Configurations;

public class CuboConfiguration : IEntityTypeConfiguration<Cubo>
{
    public void Configure(EntityTypeBuilder<Cubo> builder)
    {
        builder.ToTable("BI_CUBO");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("cub_id").UseIdentityColumn();

        builder.Property(x => x.Descripcion)
               .HasColumnName("cub_descripcion").HasMaxLength(200).IsRequired();

        builder.Property(x => x.MenuCuboId)
               .HasColumnName("mcub_id");

        builder.Property(x => x.OrigenDatos)
               .HasColumnName("cub_origendatos").HasMaxLength(2000);

        builder.Property(x => x.Observacion)
               .HasColumnName("cub_observacion").HasMaxLength(500);

        builder.Property(x => x.AmbienteId)
               .HasColumnName("amb_id").IsRequired();

        builder.Property(x => x.CuboOrigenId)
               .HasColumnName("cub_id_origen");

        builder.Property(x => x.EsSistema)
               .HasColumnName("sistema").IsRequired();

        builder.Property(x => x.FormatoId)
               .HasColumnName("for_id");

        builder.Property(x => x.UsuarioAltaId)
               .HasColumnName("usuario_alta");

        builder.Property(x => x.CreatedAt)
               .HasColumnName("fecha_alta");

        builder.Property(x => x.UpdatedAt)
               .HasColumnName("fecha_modificacion");

        builder.Property(x => x.UpdatedBy)
               .HasColumnName("usuario_modificacion");

        builder.HasIndex(x => x.UsuarioAltaId);
        builder.HasIndex(x => x.AmbienteId);
    }
}
