using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZuluIA_Back.Domain.Entities.Items;

namespace ZuluIA_Back.Infrastructure.Persistence.Configurations;

public class UnidadManipulacionConfiguration : IEntityTypeConfiguration<UnidadManipulacion>
{
    public void Configure(EntityTypeBuilder<UnidadManipulacion> builder)
    {
        builder.ToTable("ume_unidades_manipulacion");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();

        builder.Property(x => x.ItemId).HasColumnName("id_item").IsRequired();
        builder.Property(x => x.Descripcion).HasColumnName("descripcion").HasMaxLength(100).IsRequired();
        builder.Property(x => x.Cantidad).HasColumnName("cantidad").HasPrecision(18, 4).IsRequired();
        builder.Property(x => x.UnidadMedidaId).HasColumnName("id_unidad").IsRequired();
        builder.Property(x => x.TipoUnidadManipulacionId).HasColumnName("id_tipo_unidad_manipulacion");

        builder.HasIndex(x => x.ItemId);
    }
}
