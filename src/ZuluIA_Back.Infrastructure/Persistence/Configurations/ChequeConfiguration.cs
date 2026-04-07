using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZuluIA_Back.Domain.Entities.Finanzas;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Infrastructure.Persistence.Configurations;

public class ChequeConfiguration : IEntityTypeConfiguration<Cheque>
{
    public void Configure(EntityTypeBuilder<Cheque> builder)
    {
        builder.ToTable("cheques");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();

        builder.Property(x => x.CajaId)
               .HasColumnName("caja_id").IsRequired();

        builder.Property(x => x.TerceroId)
               .HasColumnName("tercero_id");

        builder.Property(x => x.NroCheque)
               .HasColumnName("nro_cheque").HasMaxLength(50).IsRequired();

        builder.Property(x => x.Banco)
               .HasColumnName("banco").HasMaxLength(100).IsRequired();

        // Campos bancarios adicionales
        builder.Property(x => x.CodigoSucursalBancaria)
               .HasColumnName("codigo_sucursal_bancaria").HasMaxLength(20);

        builder.Property(x => x.CodigoPostal)
               .HasColumnName("codigo_postal").HasMaxLength(20);

        builder.Property(x => x.Titular)
               .HasColumnName("titular").HasMaxLength(200);

        builder.Property(x => x.FechaEmision)
               .HasColumnName("fecha_emision").IsRequired();

        builder.Property(x => x.FechaVencimiento)
               .HasColumnName("fecha_vencimiento").IsRequired();

        builder.Property(x => x.FechaAcreditacion)
               .HasColumnName("fecha_acreditacion");

        builder.Property(x => x.FechaDeposito)
               .HasColumnName("fecha_deposito");

        builder.Property(x => x.Importe)
               .HasColumnName("importe").HasPrecision(18, 2).IsRequired();

        builder.Property(x => x.MonedaId)
               .HasColumnName("moneda_id").IsRequired();

        builder.Property(x => x.Estado)
               .HasColumnName("estado")
               .HasConversion(
                   v => v.ToString().ToUpperInvariant(),
                   v => Enum.Parse<EstadoCheque>(v, true))
               .HasMaxLength(20)
               .IsRequired();

        // Tipo y características
        builder.Property(x => x.Tipo)
               .HasColumnName("tipo")
               .HasConversion(
                   v => v.ToString().ToUpperInvariant(),
                   v => Enum.Parse<TipoCheque>(v, true))
               .HasMaxLength(20)
               .IsRequired();

        builder.Property(x => x.EsALaOrden)
               .HasColumnName("es_a_la_orden").IsRequired();

        builder.Property(x => x.EsCruzado)
               .HasColumnName("es_cruzado").IsRequired();

        // Referencias
        builder.Property(x => x.ChequeraId)
               .HasColumnName("chequera_id");

        builder.Property(x => x.ComprobanteOrigenId)
               .HasColumnName("comprobante_origen_id");

        // Observaciones y conceptos
        builder.Property(x => x.Observacion)
               .HasColumnName("observacion");

        builder.Property(x => x.ConceptoRechazo)
               .HasColumnName("concepto_rechazo").HasMaxLength(500);

        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");
        builder.Property(x => x.CreatedBy).HasColumnName("created_by");
        builder.Property(x => x.UpdatedBy).HasColumnName("updated_by");

        builder.HasIndex(x => x.CajaId);
        builder.HasIndex(x => x.TerceroId);
        builder.HasIndex(x => x.Estado);
        builder.HasIndex(x => x.FechaVencimiento);
        builder.HasIndex(x => x.Tipo);
        builder.HasIndex(x => x.ChequeraId);
        builder.HasIndex(x => x.ComprobanteOrigenId);
        builder.HasIndex(x => x.NroCheque);
    }
}