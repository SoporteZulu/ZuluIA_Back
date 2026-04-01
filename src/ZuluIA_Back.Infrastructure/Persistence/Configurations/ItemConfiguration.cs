using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZuluIA_Back.Domain.Entities.Items;

namespace ZuluIA_Back.Infrastructure.Persistence.Configurations;

public class ItemConfiguration : IEntityTypeConfiguration<Item>
{
    public void Configure(EntityTypeBuilder<Item> builder)
    {
        builder.ToTable("items");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
               .HasColumnName("id")
               .UseIdentityColumn();

        builder.Property(x => x.Codigo)
               .HasColumnName("codigo")
               .HasMaxLength(50) // Fusión: se amplía a 50
               .IsRequired();

        builder.Property(x => x.CodigoAlternativo)
               .HasColumnName("codigo_alternativo")
               .HasMaxLength(50);

        builder.Property(x => x.CodigoBarras)
               .HasColumnName("codigo_barras")
               .HasMaxLength(50);

        builder.Property(x => x.Descripcion)
               .HasColumnName("descripcion")
               .HasMaxLength(300) // Fusión: se amplía a 300
               .IsRequired();

        builder.Property(x => x.DescripcionAdicional)
               .HasColumnName("descripcion_adicional")
               .HasMaxLength(500); // Fusión: se amplía a 500

        builder.Property(x => x.CategoriaId)
               .HasColumnName("categoria_id");

        builder.Property(x => x.MarcaId)
               .HasColumnName("marca_id");

        builder.Property(x => x.UnidadMedidaId)
               .HasColumnName("unidad_medida_id")
               .IsRequired();

        builder.Property(x => x.AlicuotaIvaId)
               .HasColumnName("alicuota_iva_id")
               .IsRequired();

        builder.Property(x => x.AlicuotaIvaCompraId)
               .HasColumnName("alicuota_iva_compra_id");

        builder.Property(x => x.ImpuestoInternoId)
               .HasColumnName("impuesto_interno_id");

        builder.Property(x => x.MonedaId)
               .HasColumnName("moneda_id")
               .IsRequired();

        builder.Property(x => x.EsProducto)
               .HasColumnName("es_producto")
               .HasDefaultValue(true)
               .IsRequired();

        builder.Property(x => x.EsServicio)
               .HasColumnName("es_servicio")
               .HasDefaultValue(false)
               .IsRequired();

        builder.Property(x => x.EsFinanciero)
               .HasColumnName("es_financiero")
               .HasDefaultValue(false)
               .IsRequired();

        builder.Property(x => x.ManejaStock)
               .HasColumnName("maneja_stock")
               .HasDefaultValue(true)
               .IsRequired();

        builder.Property(x => x.PrecioCosto)
               .HasColumnName("precio_costo")
               .HasPrecision(18, 4)
               .HasDefaultValue(0m);

        builder.Property(x => x.PrecioVenta)
               .HasColumnName("precio_venta")
               .HasPrecision(18, 4)
               .HasDefaultValue(0m);

        builder.Property(x => x.StockMinimo)
               .HasColumnName("stock_minimo")
               .HasPrecision(18, 4)
               .HasDefaultValue(0m);

        builder.Property(x => x.StockMaximo)
               .HasColumnName("stock_maximo")
               .HasPrecision(18, 4);

        builder.Property(x => x.PuntoReposicion)
               .HasColumnName("punto_reposicion")
               .HasPrecision(18, 4);

        builder.Property(x => x.StockSeguridad)
               .HasColumnName("stock_seguridad")
               .HasPrecision(18, 4);

        builder.Property(x => x.Peso)
               .HasColumnName("peso")
               .HasPrecision(18, 4);

        builder.Property(x => x.Volumen)
               .HasColumnName("volumen")
               .HasPrecision(18, 4);

        builder.Property(x => x.EsTrazable)
               .HasColumnName("es_trazable")
               .HasDefaultValue(false)
               .IsRequired();

        builder.Property(x => x.PermiteFraccionamiento)
               .HasColumnName("permite_fraccionamiento")
               .HasDefaultValue(false)
               .IsRequired();

        builder.Property(x => x.DiasVencimientoLimite)
               .HasColumnName("dias_vencimiento_limite");

        builder.Property(x => x.DepositoDefaultId)
               .HasColumnName("deposito_default_id");

        builder.Property(x => x.CodigoAfip)
               .HasColumnName("codigo_afip")
               .HasMaxLength(50);

        builder.Property(x => x.SucursalId)
               .HasColumnName("sucursal_id");

        builder.Property(x => x.Activo)
               .HasColumnName("activo")
               .HasDefaultValue(true)
               .IsRequired();

        // ── Fase 1: Campos Esenciales de Ventas ──────────────────────────────
        builder.Property(x => x.AplicaVentas)
               .HasColumnName("aplica_ventas")
               .HasDefaultValue(true)
               .IsRequired();

        builder.Property(x => x.AplicaCompras)
               .HasColumnName("aplica_compras")
               .HasDefaultValue(true)
               .IsRequired();

        builder.Property(x => x.PorcentajeGanancia)
               .HasColumnName("porcentaje_ganancia")
               .HasPrecision(18, 4);

        builder.Property(x => x.PorcentajeMaximoDescuento)
               .HasColumnName("porcentaje_maximo_descuento")
               .HasPrecision(18, 4);

        builder.Property(x => x.EsRpt)
               .HasColumnName("es_rpt")
               .HasDefaultValue(false)
               .IsRequired();

        builder.Property(x => x.EsSistema)
               .HasColumnName("es_sistema")
               .HasDefaultValue(false)
               .IsRequired();

        // Auditoría
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");
        builder.Property(x => x.CreatedBy).HasColumnName("created_by");
        builder.Property(x => x.UpdatedBy).HasColumnName("updated_by");

        // Índices existentes
        builder.HasIndex(x => x.Codigo).IsUnique();
        builder.HasIndex(x => x.CodigoAlternativo);
        builder.HasIndex(x => x.CodigoBarras);
        builder.HasIndex(x => x.CategoriaId);
        builder.HasIndex(x => x.MarcaId);
        builder.HasIndex(x => x.Activo);
        builder.HasIndex(x => x.EsProducto);
        builder.HasIndex(x => x.EsServicio);
        
        // Índices Fase 1
        builder.HasIndex(x => x.AplicaVentas);
        builder.HasIndex(x => x.AplicaCompras);
        builder.HasIndex(x => x.EsRpt);
        builder.HasIndex(x => x.EsSistema);
        builder.HasIndex(x => x.EsTrazable);
        builder.HasIndex(x => x.DepositoDefaultId);
    }
}
