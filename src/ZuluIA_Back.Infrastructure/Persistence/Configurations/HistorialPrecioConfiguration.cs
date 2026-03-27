using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZuluIA_Back.Domain.Entities.Items;

namespace ZuluIA_Back.Infrastructure.Persistence.Configurations;

public class HistorialPrecioConfiguration : IEntityTypeConfiguration<HistorialPrecio>
{
    public void Configure(EntityTypeBuilder<HistorialPrecio> builder)
    {
        builder.ToTable("historial_precios");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedOnAdd();

        builder.Property(x => x.ItemId)
               .HasColumnName("item_id").IsRequired();

        builder.Property(x => x.PrecioCostoAnterior)
               .HasColumnName("precio_costo_anterior")
               .HasPrecision(18, 4)
               .HasDefaultValue(0m);

        builder.Property(x => x.PrecioVentaAnterior)
               .HasColumnName("precio_venta_anterior")
               .HasPrecision(18, 4)
               .HasDefaultValue(0m);

        builder.Property(x => x.PrecioCostoNuevo)
               .HasColumnName("precio_costo_nuevo")
               .HasPrecision(18, 4)
               .HasDefaultValue(0m);

        builder.Property(x => x.PrecioVentaNuevo)
               .HasColumnName("precio_venta_nuevo")
               .HasPrecision(18, 4)
               .HasDefaultValue(0m);

        builder.Property(x => x.FechaCambio)
               .HasColumnName("fecha_cambio")
               .HasDefaultValueSql("now()");

        builder.Property(x => x.UsuarioId)
               .HasColumnName("usuario_id");

        builder.Property(x => x.Motivo)
               .HasColumnName("motivo")
               .HasMaxLength(300);

        builder.HasOne<Item>()
               .WithMany()
               .HasForeignKey(x => x.ItemId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
