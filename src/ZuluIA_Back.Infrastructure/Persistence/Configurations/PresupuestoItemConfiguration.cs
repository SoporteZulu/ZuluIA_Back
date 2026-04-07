using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZuluIA_Back.Domain.Entities.Ventas;

namespace ZuluIA_Back.Infrastructure.Persistence.Configurations;

public class PresupuestoItemConfiguration : IEntityTypeConfiguration<PresupuestoItem>
{
    public void Configure(EntityTypeBuilder<PresupuestoItem> builder)
    {
        builder.ToTable("presupuestos_items");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedOnAdd();

        builder.Property(x => x.PresupuestoId)
               .HasColumnName("presupuesto_id").IsRequired();

        builder.Property(x => x.ItemId)
               .HasColumnName("item_id").IsRequired();

        builder.Property(x => x.Descripcion)
               .HasColumnName("descripcion")
               .HasMaxLength(300)
               .IsRequired();

        builder.Property(x => x.Cantidad)
               .HasColumnName("cantidad")
               .HasPrecision(18, 4)
               .IsRequired();

        builder.Property(x => x.PrecioUnitario)
               .HasColumnName("precio_unitario")
               .HasPrecision(18, 4)
               .IsRequired();

        builder.Property(x => x.DescuentoPct)
               .HasColumnName("descuento_pct")
               .HasPrecision(10, 4)
               .HasDefaultValue(0m);

        builder.Property(x => x.Subtotal)
               .HasColumnName("subtotal")
               .HasPrecision(18, 4)
               .HasDefaultValue(0m);

        builder.Property(x => x.Orden)
               .HasColumnName("orden")
               .HasDefaultValue((short)0);
    }
}
