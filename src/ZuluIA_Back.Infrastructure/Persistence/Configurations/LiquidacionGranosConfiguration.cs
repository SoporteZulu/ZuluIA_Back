using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZuluIA_Back.Domain.Entities.Agro;

namespace ZuluIA_Back.Infrastructure.Persistence.Configurations;

public class LiquidacionGranosConfiguration : IEntityTypeConfiguration<LiquidacionGranos>
{
    public void Configure(EntityTypeBuilder<LiquidacionGranos> builder)
    {
        builder.ToTable("liquidaciones_granos");
        builder.HasKey(l => l.Id);
        builder.Property(l => l.Id).HasColumnName("id").UseIdentityColumn();
        builder.Property(l => l.SucursalId).HasColumnName("sucursal_id").IsRequired();
        builder.Property(l => l.TerceroId).HasColumnName("tercero_id").IsRequired();
        builder.Property(l => l.Producto).HasColumnName("producto").HasMaxLength(200).IsRequired();
        builder.Property(l => l.Fecha).HasColumnName("fecha").IsRequired();
        builder.Property(l => l.Cantidad).HasColumnName("cantidad").HasColumnType("numeric(18,4)").IsRequired();
        builder.Property(l => l.PrecioBase).HasColumnName("precio_base").HasColumnType("numeric(18,4)").IsRequired();
        builder.Property(l => l.Deducciones).HasColumnName("deducciones").HasColumnType("numeric(18,2)").IsRequired();
        builder.Property(l => l.ValorNeto).HasColumnName("valor_neto").HasColumnType("numeric(18,2)").IsRequired();
        builder.Property(l => l.Estado).HasColumnName("estado")
            .HasConversion(v => v.ToString().ToUpperInvariant(), v => Enum.Parse<Domain.Enums.EstadoLiquidacionGranos>(v, true))
            .HasMaxLength(20).IsRequired();
        builder.Property(l => l.ComprobanteId).HasColumnName("comprobante_id");
        builder.Property(l => l.CreatedAt).HasColumnName("created_at");
        builder.Property(l => l.UpdatedAt).HasColumnName("updated_at");
        builder.Property(l => l.DeletedAt).HasColumnName("deleted_at");
        builder.Property(l => l.CreatedBy).HasColumnName("created_by");
        builder.Property(l => l.UpdatedBy).HasColumnName("updated_by");

        builder.HasMany(l => l.Conceptos).WithOne()
            .HasForeignKey(c => c.LiquidacionId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class LiquidacionGranosConceptoConfiguration : IEntityTypeConfiguration<LiquidacionGranosConcepto>
{
    public void Configure(EntityTypeBuilder<LiquidacionGranosConcepto> builder)
    {
        builder.ToTable("liquidaciones_granos_conceptos");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).HasColumnName("id").UseIdentityColumn();
        builder.Property(c => c.LiquidacionId).HasColumnName("liquidacion_id").IsRequired();
        builder.Property(c => c.Concepto).HasColumnName("concepto").HasMaxLength(200).IsRequired();
        builder.Property(c => c.Importe).HasColumnName("importe").HasColumnType("numeric(18,2)").IsRequired();
        builder.Property(c => c.EsDeduccion).HasColumnName("es_deduccion").IsRequired();
    }
}

public class CertificacionGranosConfiguration : IEntityTypeConfiguration<CertificacionGranos>
{
    public void Configure(EntityTypeBuilder<CertificacionGranos> builder)
    {
        builder.ToTable("certificaciones_granos");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).HasColumnName("id").UseIdentityColumn();
        builder.Property(c => c.LiquidacionId).HasColumnName("liquidacion_id").IsRequired();
        builder.Property(c => c.NroCertificado).HasColumnName("nro_certificado").HasMaxLength(100).IsRequired();
        builder.Property(c => c.FechaEmision).HasColumnName("fecha_emision").IsRequired();
        builder.Property(c => c.PesoNeto).HasColumnName("peso_neto").HasColumnType("numeric(18,4)").IsRequired();
        builder.Property(c => c.Humedad).HasColumnName("humedad").HasColumnType("numeric(5,2)").IsRequired();
        builder.Property(c => c.Impureza).HasColumnName("impureza").HasColumnType("numeric(5,2)").IsRequired();
        builder.Property(c => c.CalidadObservaciones).HasColumnName("calidad_observaciones").HasMaxLength(500);
        builder.Property(c => c.CreatedAt).HasColumnName("created_at");
        builder.Property(c => c.UpdatedAt).HasColumnName("updated_at");
        builder.Property(c => c.DeletedAt).HasColumnName("deleted_at");
        builder.Property(c => c.CreatedBy).HasColumnName("created_by");
        builder.Property(c => c.UpdatedBy).HasColumnName("updated_by");
    }
}
