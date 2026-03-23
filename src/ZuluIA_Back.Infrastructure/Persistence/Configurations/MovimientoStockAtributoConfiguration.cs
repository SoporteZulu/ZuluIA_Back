using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZuluIA_Back.Domain.Entities.Stock;

namespace ZuluIA_Back.Infrastructure.Persistence.Configurations;

public class MovimientoStockAtributoConfiguration : IEntityTypeConfiguration<MovimientoStockAtributo>
{
    public void Configure(EntityTypeBuilder<MovimientoStockAtributo> builder)
    {
        builder.ToTable("movimiento_stock_atributo");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();
        builder.Property(x => x.MovimientoStockId).HasColumnName("movimiento_stock_id").IsRequired();
        builder.Property(x => x.AtributoId).HasColumnName("atributo_id").IsRequired();
        builder.Property(x => x.Valor).HasColumnName("valor").HasMaxLength(200).IsRequired();
        builder.HasIndex(x => new { x.MovimientoStockId, x.AtributoId });
    }
}
