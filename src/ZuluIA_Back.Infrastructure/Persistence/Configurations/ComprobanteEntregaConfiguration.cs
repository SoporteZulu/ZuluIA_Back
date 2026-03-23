using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZuluIA_Back.Domain.Entities.Comprobantes;

namespace ZuluIA_Back.Infrastructure.Persistence.Configurations;

public class TipoEntregaConfiguration : IEntityTypeConfiguration<TipoEntrega>
{
    public void Configure(EntityTypeBuilder<TipoEntrega> builder)
    {
        builder.ToTable("tipo_entrega");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();
        builder.Property(x => x.Codigo).HasColumnName("codigo").HasMaxLength(20).IsRequired();
        builder.Property(x => x.Descripcion).HasColumnName("descripcion").HasMaxLength(150).IsRequired();
        builder.Property(x => x.TipoComprobanteId).HasColumnName("tipo_comprobante_id");
        builder.Property(x => x.Prefijo).HasColumnName("prefijo").HasMaxLength(10);
        builder.HasIndex(x => x.Codigo).IsUnique();
    }
}

public class ComprobanteEntregaConfiguration : IEntityTypeConfiguration<ComprobanteEntrega>
{
    public void Configure(EntityTypeBuilder<ComprobanteEntrega> builder)
    {
        builder.ToTable("comprobante_entrega");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();
        builder.Property(x => x.ComprobanteId).HasColumnName("comprobante_id").IsRequired();
        builder.Property(x => x.Fecha).HasColumnName("fecha").IsRequired();
        builder.Property(x => x.RazonSocial).HasColumnName("razon_social").HasMaxLength(200);
        builder.Property(x => x.Domicilio).HasColumnName("domicilio").HasMaxLength(300);
        builder.Property(x => x.LocalidadId).HasColumnName("localidad_id");
        builder.Property(x => x.ProvinciaId).HasColumnName("provincia_id");
        builder.Property(x => x.PaisId).HasColumnName("pais_id");
        builder.Property(x => x.CodigoPostal).HasColumnName("codigo_postal").HasMaxLength(15);
        builder.Property(x => x.Telefono1).HasColumnName("telefono1").HasMaxLength(50);
        builder.Property(x => x.Telefono2).HasColumnName("telefono2").HasMaxLength(50);
        builder.Property(x => x.Celular).HasColumnName("celular").HasMaxLength(50);
        builder.Property(x => x.Email).HasColumnName("email").HasMaxLength(150);
        builder.Property(x => x.Observacion).HasColumnName("observacion").HasMaxLength(500);
        builder.Property(x => x.TipoEntregaId).HasColumnName("tipo_entrega_id");
        builder.Property(x => x.TransportistaId).HasColumnName("transportista_id");
        builder.Property(x => x.ZonaId).HasColumnName("zona_id");
        builder.HasIndex(x => x.ComprobanteId).IsUnique();
    }
}

public class ComprobanteDetalleCostoConfiguration : IEntityTypeConfiguration<ComprobanteDetalleCosto>
{
    public void Configure(EntityTypeBuilder<ComprobanteDetalleCosto> builder)
    {
        builder.ToTable("comprobante_detalle_costo");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();
        builder.Property(x => x.ComprobanteItemId).HasColumnName("comprobante_item_id").IsRequired();
        builder.Property(x => x.CentroCostoId).HasColumnName("centro_costo_id").IsRequired();
        builder.Property(x => x.Procesado).HasColumnName("procesado").HasDefaultValue(false);
        builder.HasIndex(x => new { x.ComprobanteItemId, x.CentroCostoId }).IsUnique();
    }
}
