using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZuluIA_Back.Domain.Entities.Finanzas;

namespace ZuluIA_Back.Infrastructure.Persistence.Configurations;

public class CuentaCorrienteConfiguration : IEntityTypeConfiguration<CuentaCorriente>
{
    public void Configure(EntityTypeBuilder<CuentaCorriente> builder)
    {
        builder.ToTable("cuenta_corriente");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();

        builder.Property(x => x.TerceroId)
               .HasColumnName("tercero_id").IsRequired();

        builder.Property(x => x.SucursalId)
               .HasColumnName("sucursal_id");

        builder.Property(x => x.MonedaId)
               .HasColumnName("moneda_id").IsRequired();

        builder.Property(x => x.Saldo)
               .HasColumnName("saldo")
               .HasPrecision(18, 4)
               .HasDefaultValue(0m);

        builder.Property(x => x.UpdatedAt)
               .HasColumnName("updated_at");

        builder.HasIndex(x => new { x.TerceroId, x.MonedaId, x.SucursalId })
               .IsUnique();
        builder.HasIndex(x => x.TerceroId);
    }
}

public class MovimientoCtaCteConfiguration : IEntityTypeConfiguration<MovimientoCtaCte>
{
    public void Configure(EntityTypeBuilder<MovimientoCtaCte> builder)
    {
        builder.ToTable("movimientos_cta_cte");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();

        builder.Property(x => x.TerceroId)
               .HasColumnName("tercero_id").IsRequired();

        builder.Property(x => x.SucursalId)
               .HasColumnName("sucursal_id");

        builder.Property(x => x.MonedaId)
               .HasColumnName("moneda_id").IsRequired();

        builder.Property(x => x.ComprobanteId)
               .HasColumnName("comprobante_id");

        builder.Property(x => x.Fecha)
               .HasColumnName("fecha").IsRequired();

        builder.Property(x => x.Debe)
               .HasColumnName("debe")
               .HasPrecision(18, 4)
               .HasDefaultValue(0m);

        builder.Property(x => x.Haber)
               .HasColumnName("haber")
               .HasPrecision(18, 4)
               .HasDefaultValue(0m);

        builder.Property(x => x.Saldo)
               .HasColumnName("saldo")
               .HasPrecision(18, 4).IsRequired();

        builder.Property(x => x.Descripcion)
               .HasColumnName("descripcion")
               .HasMaxLength(300);

        builder.Property(x => x.CreatedAt)
               .HasColumnName("created_at");

        builder.HasIndex(x => x.TerceroId);
        builder.HasIndex(x => x.Fecha);
        builder.HasIndex(x => new { x.TerceroId, x.MonedaId });
    }
}