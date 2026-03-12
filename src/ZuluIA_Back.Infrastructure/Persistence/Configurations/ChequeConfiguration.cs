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

        builder.Property(x => x.Observacion)
               .HasColumnName("observacion");

        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Ignore(x => x.DeletedAt);
        builder.Property(x => x.CreatedBy).HasColumnName("created_by");
        builder.Property(x => x.UpdatedBy).HasColumnName("updated_by");

        builder.HasIndex(x => x.CajaId);
        builder.HasIndex(x => x.TerceroId);
        builder.HasIndex(x => x.Estado);
        builder.HasIndex(x => x.FechaVencimiento);
    }
}