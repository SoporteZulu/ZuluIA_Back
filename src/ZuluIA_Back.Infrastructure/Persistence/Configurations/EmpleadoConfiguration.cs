using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZuluIA_Back.Domain.Entities.RRHH;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Infrastructure.Persistence.Configurations;

public class EmpleadoConfiguration : IEntityTypeConfiguration<Empleado>
{
    public void Configure(EntityTypeBuilder<Empleado> builder)
    {
        builder.ToTable("empleados");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();

        builder.Property(x => x.TerceroId)
               .HasColumnName("tercero_id")
               .IsRequired();

        builder.Property(x => x.SucursalId)
               .HasColumnName("sucursal_id")
               .IsRequired();

        builder.Property(x => x.Legajo)
               .HasColumnName("legajo")
               .HasMaxLength(30)
               .IsRequired();

        builder.Property(x => x.Cargo)
               .HasColumnName("cargo")
               .HasMaxLength(100)
               .IsRequired();

        builder.Property(x => x.Area)
               .HasColumnName("area")
               .HasMaxLength(100);

        builder.Property(x => x.FechaIngreso)
               .HasColumnName("fecha_ingreso")
               .IsRequired();

        builder.Property(x => x.FechaEgreso)
               .HasColumnName("fecha_egreso");

        builder.Property(x => x.SueldoBasico)
               .HasColumnName("sueldo_basico")
               .HasPrecision(18, 4)
               .IsRequired();

        builder.Property(x => x.MonedaId)
               .HasColumnName("moneda_id")
               .IsRequired();

        builder.Property(x => x.Estado)
               .HasColumnName("estado")
               .HasConversion(
                   v => v.ToString().ToUpperInvariant(),
                   v => Enum.Parse<EstadoEmpleado>(v, true))
               .HasMaxLength(20)
               .IsRequired();

        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");

        builder.HasIndex(x => x.Legajo).IsUnique();
        builder.HasIndex(x => x.TerceroId);
        builder.HasIndex(x => x.SucursalId);
        builder.HasIndex(x => x.Estado);
    }
}

public class LiquidacionSueldoConfiguration : IEntityTypeConfiguration<LiquidacionSueldo>
{
    public void Configure(EntityTypeBuilder<LiquidacionSueldo> builder)
    {
        builder.ToTable("liquidaciones_sueldo");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();

        builder.Property(x => x.EmpleadoId)
               .HasColumnName("empleado_id")
               .IsRequired();

        builder.Property(x => x.SucursalId)
               .HasColumnName("sucursal_id")
               .IsRequired();

        builder.Property(x => x.Anio)
               .HasColumnName("anio")
               .IsRequired();

        builder.Property(x => x.Mes)
               .HasColumnName("mes")
               .IsRequired();

        builder.Property(x => x.SueldoBasico)
               .HasColumnName("sueldo_basico")
               .HasPrecision(18, 4);

        builder.Property(x => x.TotalHaberes)
               .HasColumnName("total_haberes")
               .HasPrecision(18, 4)
               .IsRequired();

        builder.Property(x => x.TotalDescuentos)
               .HasColumnName("total_descuentos")
               .HasPrecision(18, 4)
               .HasDefaultValue(0m);

        builder.Property(x => x.Neto)
               .HasColumnName("neto")
               .HasPrecision(18, 4)
               .IsRequired();

        builder.Property(x => x.MonedaId)
               .HasColumnName("moneda_id")
               .IsRequired();

        builder.Property(x => x.Pagada)
               .HasColumnName("pagada")
               .HasDefaultValue(false);

        builder.Property(x => x.Observacion)
               .HasColumnName("observacion");

        builder.Property(x => x.CreatedAt)
               .HasColumnName("created_at");

        builder.HasIndex(x => new { x.EmpleadoId, x.Anio, x.Mes }).IsUnique();
        builder.HasIndex(x => x.SucursalId);
        builder.HasIndex(x => x.Pagada);
    }
}