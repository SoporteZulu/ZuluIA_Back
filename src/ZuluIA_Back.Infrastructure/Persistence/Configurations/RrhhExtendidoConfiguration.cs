using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZuluIA_Back.Domain.Entities.RRHH;

namespace ZuluIA_Back.Infrastructure.Persistence.Configurations;

public class ComprobanteEmpleadoConfiguration : IEntityTypeConfiguration<ComprobanteEmpleado>
{
    public void Configure(EntityTypeBuilder<ComprobanteEmpleado> builder)
    {
        builder.ToTable("comprobantes_empleados");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();
        builder.Property(x => x.EmpleadoId).HasColumnName("empleado_id").IsRequired();
        builder.Property(x => x.LiquidacionSueldoId).HasColumnName("liquidacion_sueldo_id").IsRequired();
        builder.Property(x => x.SucursalId).HasColumnName("sucursal_id").IsRequired();
        builder.Property(x => x.Fecha).HasColumnName("fecha").IsRequired();
        builder.Property(x => x.Tipo).HasColumnName("tipo").HasMaxLength(40).IsRequired();
        builder.Property(x => x.Numero).HasColumnName("numero").HasMaxLength(50).IsRequired();
        builder.Property(x => x.Total).HasColumnName("total").HasPrecision(18,4).IsRequired();
        builder.Property(x => x.MonedaId).HasColumnName("moneda_id").IsRequired();
        builder.Property(x => x.Observacion).HasColumnName("observacion");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");
        builder.Property(x => x.CreatedBy).HasColumnName("created_by");
        builder.Property(x => x.UpdatedBy).HasColumnName("updated_by");
        builder.HasIndex(x => x.Numero).IsUnique();
        builder.HasIndex(x => x.LiquidacionSueldoId).IsUnique();
    }
}

public class ImputacionEmpleadoConfiguration : IEntityTypeConfiguration<ImputacionEmpleado>
{
    public void Configure(EntityTypeBuilder<ImputacionEmpleado> builder)
    {
        builder.ToTable("imputaciones_empleados");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();
        builder.Property(x => x.LiquidacionSueldoId).HasColumnName("liquidacion_sueldo_id").IsRequired();
        builder.Property(x => x.ComprobanteEmpleadoId).HasColumnName("comprobante_empleado_id");
        builder.Property(x => x.TesoreriaMovimientoId).HasColumnName("tesoreria_movimiento_id");
        builder.Property(x => x.Fecha).HasColumnName("fecha").IsRequired();
        builder.Property(x => x.Importe).HasColumnName("importe").HasPrecision(18,4).IsRequired();
        builder.Property(x => x.Observacion).HasColumnName("observacion");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");
        builder.Property(x => x.CreatedBy).HasColumnName("created_by");
        builder.Property(x => x.UpdatedBy).HasColumnName("updated_by");
        builder.HasIndex(x => x.LiquidacionSueldoId);
        builder.HasIndex(x => x.ComprobanteEmpleadoId);
    }
}
