using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZuluIA_Back.Domain.Entities.Fiscal;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Infrastructure.Persistence.Configurations;

public class CierrePeriodoContableConfiguration : IEntityTypeConfiguration<CierrePeriodoContable>
{
    public void Configure(EntityTypeBuilder<CierrePeriodoContable> builder)
    {
        builder.ToTable("cierres_periodo_contable");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();
        builder.Property(x => x.EjercicioId).HasColumnName("ejercicio_id").IsRequired();
        builder.Property(x => x.SucursalId).HasColumnName("sucursal_id");
        builder.Property(x => x.Desde).HasColumnName("desde").IsRequired();
        builder.Property(x => x.Hasta).HasColumnName("hasta").IsRequired();
        builder.Property(x => x.Observacion).HasColumnName("observacion");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");
        builder.Property(x => x.CreatedBy).HasColumnName("created_by");
        builder.Property(x => x.UpdatedBy).HasColumnName("updated_by");
        builder.HasIndex(x => new { x.EjercicioId, x.SucursalId, x.Desde, x.Hasta }).IsUnique();
    }
}

public class ReorganizacionAsientosConfiguration : IEntityTypeConfiguration<ReorganizacionAsientos>
{
    public void Configure(EntityTypeBuilder<ReorganizacionAsientos> builder)
    {
        builder.ToTable("reorganizaciones_asientos");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();
        builder.Property(x => x.EjercicioId).HasColumnName("ejercicio_id").IsRequired();
        builder.Property(x => x.SucursalId).HasColumnName("sucursal_id");
        builder.Property(x => x.Desde).HasColumnName("desde").IsRequired();
        builder.Property(x => x.Hasta).HasColumnName("hasta").IsRequired();
        builder.Property(x => x.CantidadAsientos).HasColumnName("cantidad_asientos").IsRequired();
        builder.Property(x => x.Observacion).HasColumnName("observacion");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");
        builder.Property(x => x.CreatedBy).HasColumnName("created_by");
        builder.Property(x => x.UpdatedBy).HasColumnName("updated_by");
    }
}

public class LibroViajanteRegistroConfiguration : IEntityTypeConfiguration<LibroViajanteRegistro>
{
    public void Configure(EntityTypeBuilder<LibroViajanteRegistro> builder)
    {
        builder.ToTable("libro_viajantes_registros");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();
        builder.Property(x => x.SucursalId).HasColumnName("sucursal_id").IsRequired();
        builder.Property(x => x.Desde).HasColumnName("desde").IsRequired();
        builder.Property(x => x.Hasta).HasColumnName("hasta").IsRequired();
        builder.Property(x => x.VendedorId).HasColumnName("vendedor_id");
        builder.Property(x => x.TotalVentas).HasColumnName("total_ventas").HasPrecision(18, 4).IsRequired();
        builder.Property(x => x.TotalComision).HasColumnName("total_comision").HasPrecision(18, 4).IsRequired();
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");
        builder.Property(x => x.CreatedBy).HasColumnName("created_by");
        builder.Property(x => x.UpdatedBy).HasColumnName("updated_by");
    }
}

public class RentasBsAsRegistroConfiguration : IEntityTypeConfiguration<RentasBsAsRegistro>
{
    public void Configure(EntityTypeBuilder<RentasBsAsRegistro> builder)
    {
        builder.ToTable("rentas_bs_as_registros");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();
        builder.Property(x => x.SucursalId).HasColumnName("sucursal_id").IsRequired();
        builder.Property(x => x.Desde).HasColumnName("desde").IsRequired();
        builder.Property(x => x.Hasta).HasColumnName("hasta").IsRequired();
        builder.Property(x => x.TotalPercepciones).HasColumnName("total_percepciones").HasPrecision(18, 4).IsRequired();
        builder.Property(x => x.TotalRetenciones).HasColumnName("total_retenciones").HasPrecision(18, 4).IsRequired();
        builder.Property(x => x.Observacion).HasColumnName("observacion");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");
        builder.Property(x => x.CreatedBy).HasColumnName("created_by");
        builder.Property(x => x.UpdatedBy).HasColumnName("updated_by");
    }
}

public class HechaukaRegistroConfiguration : IEntityTypeConfiguration<HechaukaRegistro>
{
    public void Configure(EntityTypeBuilder<HechaukaRegistro> builder)
    {
        builder.ToTable("hechauka_registros");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();
        builder.Property(x => x.SucursalId).HasColumnName("sucursal_id").IsRequired();
        builder.Property(x => x.Desde).HasColumnName("desde").IsRequired();
        builder.Property(x => x.Hasta).HasColumnName("hasta").IsRequired();
        builder.Property(x => x.TotalNetoGravado).HasColumnName("total_neto_gravado").HasPrecision(18, 4).IsRequired();
        builder.Property(x => x.TotalIva).HasColumnName("total_iva").HasPrecision(18, 4).IsRequired();
        builder.Property(x => x.TotalComprobantes).HasColumnName("total_comprobantes").HasPrecision(18, 4).IsRequired();
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");
        builder.Property(x => x.CreatedBy).HasColumnName("created_by");
        builder.Property(x => x.UpdatedBy).HasColumnName("updated_by");
    }
}

public class LiquidacionPrimariaGranoConfiguration : IEntityTypeConfiguration<LiquidacionPrimariaGrano>
{
    public void Configure(EntityTypeBuilder<LiquidacionPrimariaGrano> builder)
    {
        builder.ToTable("liquidaciones_primarias_granos");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();
        builder.Property(x => x.SucursalId).HasColumnName("sucursal_id").IsRequired();
        builder.Property(x => x.Fecha).HasColumnName("fecha").IsRequired();
        builder.Property(x => x.NumeroLiquidacion).HasColumnName("numero_liquidacion").HasMaxLength(50).IsRequired();
        builder.Property(x => x.Producto).HasColumnName("producto").HasMaxLength(200).IsRequired();
        builder.Property(x => x.Cantidad).HasColumnName("cantidad").HasPrecision(18, 4).IsRequired();
        builder.Property(x => x.PrecioUnitario).HasColumnName("precio_unitario").HasPrecision(18, 4).IsRequired();
        builder.Property(x => x.Total).HasColumnName("total").HasPrecision(18, 4).IsRequired();
        builder.Property(x => x.Observacion).HasColumnName("observacion");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");
        builder.Property(x => x.CreatedBy).HasColumnName("created_by");
        builder.Property(x => x.UpdatedBy).HasColumnName("updated_by");
        builder.HasIndex(x => x.NumeroLiquidacion).IsUnique();
    }
}

public class SalidaRegulatoriaConfiguration : IEntityTypeConfiguration<SalidaRegulatoria>
{
    public void Configure(EntityTypeBuilder<SalidaRegulatoria> builder)
    {
        builder.ToTable("salidas_regulatorias");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();
        builder.Property(x => x.Tipo)
            .HasColumnName("tipo")
            .HasConversion(v => v.ToString().ToUpperInvariant(), v => Enum.Parse<TipoSalidaRegulatoria>(v, true))
            .HasMaxLength(40)
            .IsRequired();
        builder.Property(x => x.SucursalId).HasColumnName("sucursal_id").IsRequired();
        builder.Property(x => x.Desde).HasColumnName("desde").IsRequired();
        builder.Property(x => x.Hasta).HasColumnName("hasta").IsRequired();
        builder.Property(x => x.Estado)
            .HasColumnName("estado")
            .HasConversion(v => v.ToString().ToUpperInvariant(), v => Enum.Parse<EstadoSalidaRegulatoria>(v, true))
            .HasMaxLength(20)
            .IsRequired();
        builder.Property(x => x.NombreArchivo).HasColumnName("nombre_archivo").HasMaxLength(200).IsRequired();
        builder.Property(x => x.Contenido).HasColumnName("contenido").IsRequired();
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");
        builder.Property(x => x.CreatedBy).HasColumnName("created_by");
        builder.Property(x => x.UpdatedBy).HasColumnName("updated_by");
        builder.HasIndex(x => x.Tipo);
    }
}
