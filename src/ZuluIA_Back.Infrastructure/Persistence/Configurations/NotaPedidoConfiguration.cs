using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZuluIA_Back.Domain.Entities.Ventas;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Infrastructure.Persistence.Configurations;

public class NotaPedidoConfiguration : IEntityTypeConfiguration<NotaPedido>
{
    public void Configure(EntityTypeBuilder<NotaPedido> builder)
    {
        builder.ToTable("notas_pedido");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();
        builder.Property(x => x.SucursalId).HasColumnName("sucursal_id").IsRequired();
        builder.Property(x => x.TerceroId).HasColumnName("tercero_id").IsRequired();
        builder.Property(x => x.Fecha).HasColumnName("fecha").IsRequired();
        builder.Property(x => x.FechaVencimiento).HasColumnName("fecha_vencimiento");
        builder.Property(x => x.Total).HasColumnName("total").HasPrecision(18, 4);
        builder.Property(x => x.Observacion).HasColumnName("observacion").HasMaxLength(500);
        builder.Property(x => x.VendedorId).HasColumnName("vendedor_id");

        builder.Property(x => x.Estado)
               .HasColumnName("estado")
               .HasConversion(v => v.ToString().ToUpperInvariant(), v => Enum.Parse<EstadoNotaPedido>(v, true))
               .HasMaxLength(20).IsRequired();

        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");
        builder.Property(x => x.CreatedBy).HasColumnName("created_by");
        builder.Property(x => x.UpdatedBy).HasColumnName("updated_by");

        builder.HasMany(x => x.Items).WithOne().HasForeignKey(x => x.NotaPedidoId).OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(x => x.SucursalId);
        builder.HasIndex(x => x.TerceroId);
    }
}

public class NotaPedidoItemConfiguration : IEntityTypeConfiguration<NotaPedidoItem>
{
    public void Configure(EntityTypeBuilder<NotaPedidoItem> builder)
    {
        builder.ToTable("notas_pedido_items");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();
        builder.Property(x => x.NotaPedidoId).HasColumnName("nota_pedido_id").IsRequired();
        builder.Property(x => x.ItemId).HasColumnName("item_id").IsRequired();
        builder.Property(x => x.Cantidad).HasColumnName("cantidad").HasPrecision(18, 4).IsRequired();
        builder.Property(x => x.CantidadPendiente).HasColumnName("cantidad_pendiente").HasPrecision(18, 4).IsRequired();
        builder.Property(x => x.PrecioUnitario).HasColumnName("precio_unitario").HasPrecision(18, 4).IsRequired();
        builder.Property(x => x.Bonificacion).HasColumnName("bonificacion").HasPrecision(5, 2);
        builder.Property(x => x.Subtotal).HasColumnName("subtotal").HasPrecision(18, 4).IsRequired();
        builder.HasIndex(x => x.NotaPedidoId);
    }
}
