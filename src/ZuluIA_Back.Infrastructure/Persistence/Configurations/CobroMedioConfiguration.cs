using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZuluIA_Back.Domain.Entities.Finanzas;

namespace ZuluIA_Back.Infrastructure.Persistence.Configurations;

public class CobroMedioConfiguration : IEntityTypeConfiguration<CobroMedio>
{
    public void Configure(EntityTypeBuilder<CobroMedio> builder)
    {
        builder.ToTable("cobros_medios");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();
        builder.Property(x => x.CobroId).HasColumnName("cobro_id").IsRequired();
        builder.Property(x => x.CajaId).HasColumnName("caja_id").IsRequired();
        builder.Property(x => x.FormaPagoId).HasColumnName("forma_pago_id").IsRequired();
        builder.Property(x => x.ChequeId).HasColumnName("cheque_id");
        builder.Property(x => x.Importe).HasColumnName("importe").HasPrecision(18, 2);
        builder.Property(x => x.MonedaId).HasColumnName("moneda_id").IsRequired();
        builder.Property(x => x.Cotizacion).HasColumnName("cotizacion").HasPrecision(18, 6).HasDefaultValue(1m);

        builder.HasIndex(x => x.CobroId);
    }
}