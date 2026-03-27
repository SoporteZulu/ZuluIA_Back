using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZuluIA_Back.Domain.Entities.Colegio;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Infrastructure.Persistence.Configurations;

public class PlanGeneralColegioConfiguration : IEntityTypeConfiguration<PlanGeneralColegio>
{
    public void Configure(EntityTypeBuilder<PlanGeneralColegio> builder)
    {
        builder.ToTable("colegio_planes_generales");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();
        builder.Property(x => x.SucursalId).HasColumnName("sucursal_id").IsRequired();
        builder.Property(x => x.PlanPagoId).HasColumnName("plan_pago_id").IsRequired();
        builder.Property(x => x.TipoComprobanteId).HasColumnName("tipo_comprobante_id").IsRequired();
        builder.Property(x => x.ItemId).HasColumnName("item_id").IsRequired();
        builder.Property(x => x.MonedaId).HasColumnName("moneda_id").IsRequired();
        builder.Property(x => x.Codigo).HasColumnName("codigo").HasMaxLength(50).IsRequired();
        builder.Property(x => x.Descripcion).HasColumnName("descripcion").HasMaxLength(200).IsRequired();
        builder.Property(x => x.ImporteBase).HasColumnName("importe_base").HasPrecision(18, 4).IsRequired();
        builder.Property(x => x.Activo).HasColumnName("activo").IsRequired();
        builder.Property(x => x.Observacion).HasColumnName("observacion");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");
        builder.Property(x => x.CreatedBy).HasColumnName("created_by");
        builder.Property(x => x.UpdatedBy).HasColumnName("updated_by");
        builder.HasIndex(x => new { x.SucursalId, x.Codigo }).IsUnique();
    }
}

public class LoteColegioConfiguration : IEntityTypeConfiguration<LoteColegio>
{
    public void Configure(EntityTypeBuilder<LoteColegio> builder)
    {
        builder.ToTable("colegio_lotes");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();
        builder.Property(x => x.PlanGeneralColegioId).HasColumnName("plan_general_colegio_id").IsRequired();
        builder.Property(x => x.Codigo).HasColumnName("codigo").HasMaxLength(50).IsRequired();
        builder.Property(x => x.FechaEmision).HasColumnName("fecha_emision").IsRequired();
        builder.Property(x => x.FechaVencimiento).HasColumnName("fecha_vencimiento").IsRequired();
        builder.Property(x => x.Estado)
            .HasColumnName("estado")
            .HasConversion(v => v.ToString().ToUpperInvariant(), v => Enum.Parse<EstadoLoteColegio>(v, true))
            .HasMaxLength(20)
            .IsRequired();
        builder.Property(x => x.CantidadCedulones).HasColumnName("cantidad_cedulones").IsRequired();
        builder.Property(x => x.Observacion).HasColumnName("observacion");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");
        builder.Property(x => x.CreatedBy).HasColumnName("created_by");
        builder.Property(x => x.UpdatedBy).HasColumnName("updated_by");
        builder.HasIndex(x => x.Codigo).IsUnique();
    }
}

public class CobinproColegioOperacionConfiguration : IEntityTypeConfiguration<CobinproColegioOperacion>
{
    public void Configure(EntityTypeBuilder<CobinproColegioOperacion> builder)
    {
        builder.ToTable("colegio_cobinpro_operaciones");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();
        builder.Property(x => x.CedulonId).HasColumnName("cedulon_id").IsRequired();
        builder.Property(x => x.TerceroId).HasColumnName("tercero_id").IsRequired();
        builder.Property(x => x.SucursalId).HasColumnName("sucursal_id").IsRequired();
        builder.Property(x => x.CobroId).HasColumnName("cobro_id");
        builder.Property(x => x.Fecha).HasColumnName("fecha").IsRequired();
        builder.Property(x => x.Importe).HasColumnName("importe").HasPrecision(18, 4).IsRequired();
        builder.Property(x => x.ReferenciaExterna).HasColumnName("referencia_externa").HasMaxLength(100).IsRequired();
        builder.Property(x => x.Estado)
            .HasColumnName("estado")
            .HasConversion(v => v.ToString().ToUpperInvariant(), v => Enum.Parse<EstadoCobinproColegio>(v, true))
            .HasMaxLength(20)
            .IsRequired();
        builder.Property(x => x.Observacion).HasColumnName("observacion");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");
        builder.Property(x => x.CreatedBy).HasColumnName("created_by");
        builder.Property(x => x.UpdatedBy).HasColumnName("updated_by");
        builder.HasIndex(x => x.ReferenciaExterna).IsUnique();
    }
}

public class ColegioReciboDetalleConfiguration : IEntityTypeConfiguration<ColegioReciboDetalle>
{
    public void Configure(EntityTypeBuilder<ColegioReciboDetalle> builder)
    {
        builder.ToTable("colegio_recibos_detalles");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();
        builder.Property(x => x.CobroId).HasColumnName("cobro_id").IsRequired();
        builder.Property(x => x.CedulonId).HasColumnName("cedulon_id").IsRequired();
        builder.Property(x => x.Importe).HasColumnName("importe").HasPrecision(18, 4).IsRequired();
        builder.HasIndex(x => x.CobroId);
        builder.HasIndex(x => x.CedulonId);
    }
}
