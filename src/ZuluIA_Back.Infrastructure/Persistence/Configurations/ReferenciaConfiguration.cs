using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZuluIA_Back.Domain.Entities.Referencia;

namespace ZuluIA_Back.Infrastructure.Persistence.Configurations;

public class MonedaConfiguration : IEntityTypeConfiguration<Moneda>
{
    public void Configure(EntityTypeBuilder<Moneda> builder)
    {
        builder.ToTable("monedas");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();
        builder.Property(x => x.Codigo).HasColumnName("codigo").HasMaxLength(10).IsRequired();
        builder.Property(x => x.Descripcion).HasColumnName("descripcion").HasMaxLength(100).IsRequired();
        builder.Property(x => x.Simbolo).HasColumnName("simbolo").HasMaxLength(5).HasDefaultValue("$");
        builder.Property(x => x.SinDecimales).HasColumnName("sin_decimales").HasDefaultValue(false);
        builder.Property(x => x.Activa).HasColumnName("activa").HasDefaultValue(true);
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Ignore(x => x.DeletedAt);
        builder.Ignore(x => x.CreatedBy);
        builder.Ignore(x => x.UpdatedBy);
        builder.HasIndex(x => x.Codigo).IsUnique();
    }
}

public class CondicionIvaConfiguration : IEntityTypeConfiguration<CondicionIva>
{
    public void Configure(EntityTypeBuilder<CondicionIva> builder)
    {
        builder.ToTable("condiciones_iva");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();
        builder.Property(x => x.Codigo).HasColumnName("codigo").IsRequired();
        builder.Property(x => x.Descripcion).HasColumnName("descripcion").HasMaxLength(100).IsRequired();
        builder.HasIndex(x => x.Codigo).IsUnique();
    }
}

public class TipoDocumentoConfiguration : IEntityTypeConfiguration<TipoDocumento>
{
    public void Configure(EntityTypeBuilder<TipoDocumento> builder)
    {
        builder.ToTable("tipos_documento");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();
        builder.Property(x => x.Codigo).HasColumnName("codigo").IsRequired();
        builder.Property(x => x.Descripcion).HasColumnName("descripcion").HasMaxLength(100).IsRequired();
        builder.HasIndex(x => x.Codigo).IsUnique();
    }
}

public class TipoComprobanteConfiguration : IEntityTypeConfiguration<TipoComprobante>
{
    public void Configure(EntityTypeBuilder<TipoComprobante> builder)
    {
        builder.ToTable("tipos_comprobante");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();
        builder.Property(x => x.Codigo).HasColumnName("codigo").HasMaxLength(10).IsRequired();
        builder.Property(x => x.Descripcion).HasColumnName("descripcion").HasMaxLength(100).IsRequired();
        builder.Property(x => x.EsVenta).HasColumnName("es_venta").HasDefaultValue(true);
        builder.Property(x => x.EsCompra).HasColumnName("es_compra").HasDefaultValue(false);
        builder.Property(x => x.EsInterno).HasColumnName("es_interno").HasDefaultValue(false);
        builder.Property(x => x.AfectaStock).HasColumnName("afecta_stock").HasDefaultValue(false);
        builder.Property(x => x.AfectaCuentaCorriente).HasColumnName("afecta_cta_cte").HasDefaultValue(true);
        builder.Property(x => x.GeneraAsiento).HasColumnName("genera_asiento").HasDefaultValue(true);
        builder.Property(x => x.TipoAfip).HasColumnName("tipo_afip");
        builder.Property(x => x.LetraAfip).HasColumnName("letra_afip").HasMaxLength(1);
        builder.Property(x => x.Activo).HasColumnName("activo").HasDefaultValue(true);
        builder.HasIndex(x => x.Codigo).IsUnique();
    }
}

public class AlicuotaIvaConfiguration : IEntityTypeConfiguration<AlicuotaIva>
{
    public void Configure(EntityTypeBuilder<AlicuotaIva> builder)
    {
        builder.ToTable("alicuotas_iva");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();
        builder.Property(x => x.Codigo).HasColumnName("codigo").IsRequired();
        builder.Property(x => x.Descripcion).HasColumnName("descripcion").HasMaxLength(100).IsRequired();
        builder.Property(x => x.Porcentaje).HasColumnName("porcentaje").HasPrecision(5, 2).IsRequired();
        builder.HasIndex(x => x.Codigo).IsUnique();
    }
}

public class UnidadMedidaConfiguration : IEntityTypeConfiguration<UnidadMedida>
{
    public void Configure(EntityTypeBuilder<UnidadMedida> builder)
    {
        builder.ToTable("unidades_medida");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();
        builder.Property(x => x.Codigo).HasColumnName("codigo").HasMaxLength(10).IsRequired();
        builder.Property(x => x.Descripcion).HasColumnName("descripcion").HasMaxLength(100).IsRequired();
        builder.HasIndex(x => x.Codigo).IsUnique();
    }
}

public class FormaPagoConfiguration : IEntityTypeConfiguration<FormaPago>
{
    public void Configure(EntityTypeBuilder<FormaPago> builder)
    {
        builder.ToTable("formas_pago");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();
        builder.Property(x => x.Descripcion).HasColumnName("descripcion").HasMaxLength(100).IsRequired();
        builder.Property(x => x.Activa).HasColumnName("activa").HasDefaultValue(true);
    }
}

public class CategoriaTerceroConfiguration : IEntityTypeConfiguration<CategoriaTercero>
{
    public void Configure(EntityTypeBuilder<CategoriaTercero> builder)
    {
        builder.ToTable("categorias_terceros");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();
        builder.Property(x => x.Descripcion).HasColumnName("descripcion").HasMaxLength(100).IsRequired();
        builder.Property(x => x.EsImportador).HasColumnName("es_importador").HasDefaultValue(false);
    }
}