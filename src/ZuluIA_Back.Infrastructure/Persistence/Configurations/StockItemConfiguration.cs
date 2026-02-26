using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZuluIA_Back.Domain.Entities.Stock;

namespace ZuluIA_Back.Infrastructure.Persistence.Configurations;

public class StockItemConfiguration : IEntityTypeConfiguration<StockItem>
{
    public void Configure(EntityTypeBuilder<StockItem> builder)
    {
        builder.ToTable("stock");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();
        builder.Property(x => x.ItemId).HasColumnName("item_id").IsRequired();
        builder.Property(x => x.DepositoId).HasColumnName("deposito_id").IsRequired();
        builder.Property(x => x.Cantidad).HasColumnName("cantidad").HasPrecision(18, 4);
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");

        builder.HasIndex(x => new { x.ItemId, x.DepositoId }).IsUnique();
    }
}