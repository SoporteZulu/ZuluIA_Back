using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZuluIA_Back.Domain.Entities.Logistica;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Infrastructure.Persistence.Configurations;

public class OrdenPreparacionConfiguration : IEntityTypeConfiguration<OrdenPreparacion>
{
    public void Configure(EntityTypeBuilder<OrdenPreparacion> builder)
    {
        builder.ToTable("ordenes_preparacion");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();
        builder.Property(x => x.SucursalId).HasColumnName("sucursal_id").IsRequired();
        builder.Property(x => x.ComprobanteOrigenId).HasColumnName("comprobante_origen_id");
        builder.Property(x => x.TerceroId).HasColumnName("tercero_id");
        builder.Property(x => x.Fecha).HasColumnName("fecha").IsRequired();
        builder.Property(x => x.Estado)
            .HasColumnName("estado")
            .HasConversion(v => v.ToString().ToUpperInvariant(), v => Enum.Parse<EstadoOrdenPreparacion>(v, true))
            .HasMaxLength(20)
            .IsRequired();
        builder.Property(x => x.Observacion).HasColumnName("observacion");
        builder.Property(x => x.FechaConfirmacion).HasColumnName("fecha_confirmacion");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");
        builder.Property(x => x.CreatedBy).HasColumnName("created_by");
        builder.Property(x => x.UpdatedBy).HasColumnName("updated_by");
        builder.HasMany(x => x.Detalles).WithOne().HasForeignKey(x => x.OrdenPreparacionId).OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(x => x.SucursalId);
        builder.HasIndex(x => x.TerceroId);
        builder.HasIndex(x => x.Estado);
        builder.HasIndex(x => x.Fecha);
    }
}

public class OrdenPreparacionDetalleConfiguration : IEntityTypeConfiguration<OrdenPreparacionDetalle>
{
    public void Configure(EntityTypeBuilder<OrdenPreparacionDetalle> builder)
    {
        builder.ToTable("ordenes_preparacion_detalles");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();
        builder.Property(x => x.OrdenPreparacionId).HasColumnName("orden_preparacion_id").IsRequired();
        builder.Property(x => x.ItemId).HasColumnName("item_id").IsRequired();
        builder.Property(x => x.DepositoId).HasColumnName("deposito_id").IsRequired();
        builder.Property(x => x.Cantidad).HasColumnName("cantidad").HasPrecision(18,4).IsRequired();
        builder.Property(x => x.CantidadEntregada).HasColumnName("cantidad_entregada").HasPrecision(18,4).IsRequired();
        builder.Property(x => x.Observacion).HasColumnName("observacion");
        builder.HasIndex(x => x.OrdenPreparacionId);
    }
}

public class TransferenciaDepositoConfiguration : IEntityTypeConfiguration<TransferenciaDeposito>
{
    public void Configure(EntityTypeBuilder<TransferenciaDeposito> builder)
    {
        builder.ToTable("transferencias_deposito");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();
        builder.Property(x => x.OrdenPreparacionId).HasColumnName("orden_preparacion_id");
        builder.Property(x => x.SucursalId).HasColumnName("sucursal_id").IsRequired();
        builder.Property(x => x.DepositoOrigenId).HasColumnName("deposito_origen_id").IsRequired();
        builder.Property(x => x.DepositoDestinoId).HasColumnName("deposito_destino_id").IsRequired();
        builder.Property(x => x.Fecha).HasColumnName("fecha").IsRequired();
        builder.Property(x => x.Estado)
            .HasColumnName("estado")
            .HasConversion(v => v.ToString().ToUpperInvariant(), v => Enum.Parse<EstadoTransferenciaDeposito>(v, true))
            .HasMaxLength(20)
            .IsRequired();
        builder.Property(x => x.Observacion).HasColumnName("observacion");
        builder.Property(x => x.FechaConfirmacion).HasColumnName("fecha_confirmacion");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");
        builder.Property(x => x.CreatedBy).HasColumnName("created_by");
        builder.Property(x => x.UpdatedBy).HasColumnName("updated_by");
        builder.HasMany(x => x.Detalles).WithOne().HasForeignKey(x => x.TransferenciaDepositoId).OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(x => x.OrdenPreparacionId);
        builder.HasIndex(x => x.SucursalId);
        builder.HasIndex(x => x.Estado);
        builder.HasIndex(x => x.Fecha);
    }
}

public class TransferenciaDepositoDetalleConfiguration : IEntityTypeConfiguration<TransferenciaDepositoDetalle>
{
    public void Configure(EntityTypeBuilder<TransferenciaDepositoDetalle> builder)
    {
        builder.ToTable("transferencias_deposito_detalles");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();
        builder.Property(x => x.TransferenciaDepositoId).HasColumnName("transferencia_deposito_id").IsRequired();
        builder.Property(x => x.ItemId).HasColumnName("item_id").IsRequired();
        builder.Property(x => x.Cantidad).HasColumnName("cantidad").HasPrecision(18,4).IsRequired();
        builder.Property(x => x.Observacion).HasColumnName("observacion");
        builder.HasIndex(x => x.TransferenciaDepositoId);
    }
}

public class LogisticaInternaEventoConfiguration : IEntityTypeConfiguration<LogisticaInternaEvento>
{
    public void Configure(EntityTypeBuilder<LogisticaInternaEvento> builder)
    {
        builder.ToTable("logistica_interna_eventos");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();
        builder.Property(x => x.OrdenPreparacionId).HasColumnName("orden_preparacion_id");
        builder.Property(x => x.TransferenciaDepositoId).HasColumnName("transferencia_deposito_id");
        builder.Property(x => x.TipoEvento)
            .HasColumnName("tipo_evento")
            .HasConversion(v => v.ToString().ToUpperInvariant(), v => Enum.Parse<TipoEventoLogisticaInterna>(v, true))
            .HasMaxLength(40)
            .IsRequired();
        builder.Property(x => x.Fecha).HasColumnName("fecha").IsRequired();
        builder.Property(x => x.Descripcion).HasColumnName("descripcion").HasMaxLength(300).IsRequired();
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");
        builder.Property(x => x.CreatedBy).HasColumnName("created_by");
        builder.Property(x => x.UpdatedBy).HasColumnName("updated_by");
        builder.HasIndex(x => x.OrdenPreparacionId);
        builder.HasIndex(x => x.TransferenciaDepositoId);
        builder.HasIndex(x => x.TipoEvento);
    }
}
