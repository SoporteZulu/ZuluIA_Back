using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZuluIA_Back.Domain.Entities.Stock;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Infrastructure.Persistence.Configurations;

public class MovimientoStockConfiguration : IEntityTypeConfiguration<MovimientoStock>
{
    public void Configure(EntityTypeBuilder<MovimientoStock> builder)
    {
        builder.ToTable("movimientos_stock");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();
        builder.Property(x => x.ItemId).HasColumnName("item_id").IsRequired();
        builder.Property(x => x.DepositoId).HasColumnName("deposito_id").IsRequired();
        builder.Property(x => x.Fecha).HasColumnName("fecha").IsRequired();
        builder.Property(x => x.TipoMovimiento)
               .HasColumnName("tipo_movimiento")
               .HasConversion(
                   v => v.ToString().ToUpperInvariant(),
                   v => Enum.Parse<TipoMovimientoStock>(v, true))
               .HasMaxLength(30)
               .IsRequired();
        builder.Property(x => x.Cantidad).HasColumnName("cantidad").HasPrecision(18, 4);
        builder.Property(x => x.SaldoResultante).HasColumnName("saldo_resultante").HasPrecision(18, 4);
        builder.Property(x => x.OrigenTabla).HasColumnName("origen_tabla").HasMaxLength(50);
        builder.Property(x => x.OrigenId).HasColumnName("origen_id");
        builder.Property(x => x.Observacion).HasColumnName("observacion").HasMaxLength(300);
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.CreatedBy).HasColumnName("created_by");

        builder.HasIndex(x => x.ItemId);
        builder.HasIndex(x => x.Fecha);
    }
}