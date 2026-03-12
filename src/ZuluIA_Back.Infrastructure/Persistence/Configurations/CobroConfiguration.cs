using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZuluIA_Back.Domain.Entities.Finanzas;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Infrastructure.Persistence.Configurations;

public class CobroConfiguration : IEntityTypeConfiguration<Cobro>
{
    public void Configure(EntityTypeBuilder<Cobro> builder)
    {
        builder.ToTable("cobros");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();

        builder.Property(x => x.SucursalId)
               .HasColumnName("sucursal_id").IsRequired();

        builder.Property(x => x.TerceroId)
               .HasColumnName("tercero_id").IsRequired();

        builder.Property(x => x.Fecha)
               .HasColumnName("fecha").IsRequired();

        builder.Property(x => x.MonedaId)
               .HasColumnName("moneda_id").IsRequired();

        builder.Property(x => x.Cotizacion)
               .HasColumnName("cotizacion")
               .HasPrecision(18, 6)
               .HasDefaultValue(1m);

        builder.Property(x => x.Total)
               .HasColumnName("total")
               .HasPrecision(18, 4);

        builder.Property(x => x.Observacion)
               .HasColumnName("observacion")
               .HasMaxLength(500);

        builder.Property(x => x.Estado)
               .HasColumnName("estado")
               .HasConversion(
                   v => v.ToString().ToUpperInvariant(),
                   v => Enum.Parse<EstadoCobro>(v, true))
               .HasMaxLength(20)
               .IsRequired();

        builder.Property(x => x.NroCierre)
               .HasColumnName("nro_cierre");

        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");
        builder.Property(x => x.CreatedBy).HasColumnName("created_by");
        builder.Ignore(x => x.UpdatedBy);

        builder.HasMany(x => x.Medios)
               .WithOne()
               .HasForeignKey(x => x.CobroId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.SucursalId);
        builder.HasIndex(x => x.TerceroId);
        builder.HasIndex(x => x.Fecha);
        builder.HasIndex(x => x.Estado);
    }
}

public class CobroMedioConfiguration : IEntityTypeConfiguration<CobroMedio>
{
    public void Configure(EntityTypeBuilder<CobroMedio> builder)
    {
        builder.ToTable("cobros_medios");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();

        builder.Property(x => x.CobroId)
               .HasColumnName("cobro_id").IsRequired();

        builder.Property(x => x.CajaId)
               .HasColumnName("caja_id").IsRequired();

        builder.Property(x => x.FormaPagoId)
               .HasColumnName("forma_pago_id").IsRequired();

        builder.Property(x => x.ChequeId)
               .HasColumnName("cheque_id");

        builder.Property(x => x.Importe)
               .HasColumnName("importe")
               .HasPrecision(18, 4).IsRequired();

        builder.Property(x => x.MonedaId)
               .HasColumnName("moneda_id").IsRequired();

        builder.Property(x => x.Cotizacion)
               .HasColumnName("cotizacion")
               .HasPrecision(18, 6)
               .HasDefaultValue(1m);

        builder.HasIndex(x => x.CobroId);
        builder.HasIndex(x => x.CajaId);
    }
}
