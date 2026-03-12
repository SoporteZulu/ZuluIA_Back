using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZuluIA_Back.Domain.Entities.Contabilidad;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Infrastructure.Persistence.Configurations;

public class AsientoConfiguration : IEntityTypeConfiguration<Asiento>
{
    public void Configure(EntityTypeBuilder<Asiento> builder)
    {
        builder.ToTable("asientos");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();

        builder.Property(x => x.EjercicioId)
               .HasColumnName("ejercicio_id")
               .IsRequired();

        builder.Property(x => x.SucursalId)
               .HasColumnName("sucursal_id")
               .IsRequired();

        builder.Property(x => x.Fecha)
               .HasColumnName("fecha")
               .IsRequired();

        builder.Property(x => x.Numero)
               .HasColumnName("numero")
               .IsRequired();

        builder.Property(x => x.Descripcion)
               .HasColumnName("descripcion")
               .HasMaxLength(500) // Mantener longitud máxima para textos largos
               .IsRequired();

        builder.Property(x => x.OrigenTabla)
               .HasColumnName("origen_tabla")
               .HasMaxLength(100);

        builder.Property(x => x.OrigenId)
               .HasColumnName("origen_id");

        builder.Property(x => x.Estado)
               .HasColumnName("estado")
               .HasConversion(
                   v => v.ToString().ToUpperInvariant(),
                   v => Enum.Parse<EstadoAsiento>(v, true))
               .HasMaxLength(20)
               .IsRequired();

        builder.Ignore(x => x.Cuadra);

        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.CreatedBy).HasColumnName("created_by");
        builder.Property(x => x.UpdatedBy).HasColumnName("updated_by");
        builder.Property<DateTimeOffset?>("deleted_at").HasColumnName("deleted_at");

        builder.HasMany(x => x.Lineas)
               .WithOne()
               .HasForeignKey(x => x.AsientoId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.EjercicioId, x.SucursalId, x.Numero })
               .IsUnique();
        builder.HasIndex(x => x.Fecha);
        builder.HasIndex(x => x.Estado);
        builder.HasIndex(x => new { x.OrigenTabla, x.OrigenId });
    }
}

public class AsientoLineaConfiguration : IEntityTypeConfiguration<AsientoLinea>
{
    public void Configure(EntityTypeBuilder<AsientoLinea> builder)
    {
        builder.ToTable("asientos_lineas");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();

        builder.Property(x => x.AsientoId)
               .HasColumnName("asiento_id")
               .IsRequired();

        builder.Property(x => x.CuentaId)
               .HasColumnName("cuenta_id")
               .IsRequired();

        builder.Property(x => x.Debe)
               .HasColumnName("debe")
               .HasPrecision(18, 4)
               .HasDefaultValue(0m);

        builder.Property(x => x.Haber)
               .HasColumnName("haber")
               .HasPrecision(18, 4)
               .HasDefaultValue(0m);

        builder.Property(x => x.Descripcion)
               .HasColumnName("descripcion")
               .HasMaxLength(300);

        builder.Property(x => x.Orden)
               .HasColumnName("orden")
               .HasDefaultValue((short)0);

        builder.Property(x => x.CentroCostoId)
               .HasColumnName("centro_costo_id");

        builder.HasIndex(x => x.AsientoId);
        builder.HasIndex(x => x.CuentaId);
        builder.HasIndex(x => x.CentroCostoId);
    }
}
