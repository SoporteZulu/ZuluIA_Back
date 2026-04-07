using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZuluIA_Back.Domain.Entities.Precios;

namespace ZuluIA_Back.Infrastructure.Persistence.Configurations;

public class ListaPreciosPromocionConfiguration : IEntityTypeConfiguration<ListaPreciosPromocion>
{
    public void Configure(EntityTypeBuilder<ListaPreciosPromocion> builder)
    {
        builder.ToTable("listas_precios_promociones");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();

        builder.Property(x => x.ListaId)
               .HasColumnName("lista_id")
               .IsRequired();

        builder.Property(x => x.Descripcion)
               .HasColumnName("descripcion")
               .HasMaxLength(200)
               .IsRequired();

        builder.Property(x => x.DescuentoPct)
               .HasColumnName("descuento_pct")
               .HasPrecision(5, 2)
               .IsRequired();

        builder.Property(x => x.VigenciaDesde)
               .HasColumnName("vigencia_desde")
               .IsRequired();

        builder.Property(x => x.VigenciaHasta)
               .HasColumnName("vigencia_hasta")
               .IsRequired();

        builder.Property(x => x.Activa)
               .HasColumnName("activa")
               .HasDefaultValue(true);

        builder.Property(x => x.ItemId)
               .HasColumnName("item_id");

        builder.Property(x => x.CategoriaId)
               .HasColumnName("categoria_id");

        builder.Property(x => x.MontoMinimoCompra)
               .HasColumnName("monto_minimo_compra")
               .HasPrecision(18, 2);

        builder.Property(x => x.CantidadMinima)
               .HasColumnName("cantidad_minima");

        builder.Property(x => x.Observaciones)
               .HasColumnName("observaciones")
               .HasMaxLength(500);

        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");
        builder.Property(x => x.CreatedBy).HasColumnName("created_by");
        builder.Property(x => x.UpdatedBy).HasColumnName("updated_by");

        builder.HasOne(x => x.Lista)
               .WithMany()
               .HasForeignKey(x => x.ListaId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.ListaId);
        builder.HasIndex(x => x.ItemId);
        builder.HasIndex(x => x.CategoriaId);
        builder.HasIndex(x => x.Activa);
        builder.HasIndex(x => new { x.VigenciaDesde, x.VigenciaHasta });
    }
}
