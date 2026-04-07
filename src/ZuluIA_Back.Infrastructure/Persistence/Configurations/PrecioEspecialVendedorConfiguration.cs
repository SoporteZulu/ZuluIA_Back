using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZuluIA_Back.Domain.Entities.Precios;

namespace ZuluIA_Back.Infrastructure.Persistence.Configurations;

public class PrecioEspecialVendedorConfiguration : IEntityTypeConfiguration<PrecioEspecialVendedor>
{
    public void Configure(EntityTypeBuilder<PrecioEspecialVendedor> builder)
    {
        builder.ToTable("precios_especiales_vendedores");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();

        builder.Property(x => x.ItemId)
               .HasColumnName("item_id")
               .IsRequired();

        builder.Property(x => x.VendedorId)
               .HasColumnName("vendedor_id")
               .IsRequired();

        builder.Property(x => x.MonedaId)
               .HasColumnName("moneda_id")
               .IsRequired();

        builder.Property(x => x.Precio)
               .HasColumnName("precio")
               .HasPrecision(18, 4)
               .IsRequired();

        builder.Property(x => x.DescuentoPct)
               .HasColumnName("descuento_pct")
               .HasPrecision(5, 2)
               .HasDefaultValue(0);

        builder.Property(x => x.VigenciaDesde)
               .HasColumnName("vigencia_desde");

        builder.Property(x => x.VigenciaHasta)
               .HasColumnName("vigencia_hasta");

        builder.Property(x => x.Activo)
               .HasColumnName("activo")
               .HasDefaultValue(true);

        builder.Property(x => x.Observaciones)
               .HasColumnName("observaciones")
               .HasMaxLength(500);

        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");
        builder.Property(x => x.CreatedBy).HasColumnName("created_by");
        builder.Property(x => x.UpdatedBy).HasColumnName("updated_by");

        builder.HasIndex(x => new { x.ItemId, x.VendedorId, x.MonedaId })
               .IsUnique()
               .HasFilter("deleted_at IS NULL");

        builder.HasIndex(x => x.ItemId);
        builder.HasIndex(x => x.VendedorId);
        builder.HasIndex(x => x.MonedaId);
        builder.HasIndex(x => x.Activo);
        builder.HasIndex(x => new { x.VigenciaDesde, x.VigenciaHasta });
    }
}
