using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZuluIA_Back.Domain.Entities.Produccion;

namespace ZuluIA_Back.Infrastructure.Persistence.Configurations;

public class OrdenTrabajoConsumoConfiguration : IEntityTypeConfiguration<OrdenTrabajoConsumo>
{
    public void Configure(EntityTypeBuilder<OrdenTrabajoConsumo> builder)
    {
        builder.ToTable("ordenes_trabajo_consumos");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();
        builder.Property(x => x.OrdenTrabajoId).HasColumnName("orden_trabajo_id").IsRequired();
        builder.Property(x => x.ItemId).HasColumnName("item_id").IsRequired();
        builder.Property(x => x.DepositoId).HasColumnName("deposito_id").IsRequired();
        builder.Property(x => x.CantidadPlanificada).HasColumnName("cantidad_planificada").HasPrecision(18, 4).IsRequired();
        builder.Property(x => x.CantidadConsumida).HasColumnName("cantidad_consumida").HasPrecision(18, 4).IsRequired();
        builder.Property(x => x.MovimientoStockId).HasColumnName("movimiento_stock_id");
        builder.Property(x => x.Observacion).HasColumnName("observacion").HasMaxLength(500);
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");
        builder.Property(x => x.CreatedBy).HasColumnName("created_by");
        builder.Property(x => x.UpdatedBy).HasColumnName("updated_by");
        builder.HasIndex(x => x.OrdenTrabajoId);
        builder.HasIndex(x => x.ItemId);
    }
}
