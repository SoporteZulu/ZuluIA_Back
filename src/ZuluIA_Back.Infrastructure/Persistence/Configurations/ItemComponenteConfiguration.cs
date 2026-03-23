using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZuluIA_Back.Domain.Entities.Items;

namespace ZuluIA_Back.Infrastructure.Persistence.Configurations;

public class ItemComponenteConfiguration : IEntityTypeConfiguration<ItemComponente>
{
    public void Configure(EntityTypeBuilder<ItemComponente> builder)
    {
        builder.ToTable("item_componente");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();
        builder.Property(x => x.ItemPadreId).HasColumnName("item_padre_id").IsRequired();
        builder.Property(x => x.ComponenteId).HasColumnName("componente_id").IsRequired();
        builder.Property(x => x.Cantidad).HasColumnName("cantidad").HasColumnType("numeric(18,4)").IsRequired();
        builder.Property(x => x.UnidadMedidaId).HasColumnName("unidad_medida_id");
        builder.HasIndex(x => new { x.ItemPadreId, x.ComponenteId }).IsUnique();
    }
}
