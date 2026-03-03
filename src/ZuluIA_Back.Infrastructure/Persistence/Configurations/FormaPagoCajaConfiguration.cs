using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZuluIA_Back.Domain.Entities.Finanzas;

namespace ZuluIA_Back.Infrastructure.Persistence.Configurations;

public class FormaPagoCajaConfiguration : IEntityTypeConfiguration<FormaPagoCaja>
{
    public void Configure(EntityTypeBuilder<FormaPagoCaja> builder)
    {
        builder.ToTable("forma_pago_caja");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();

        builder.Property(x => x.CajaId)
               .HasColumnName("caja_id").IsRequired();

        builder.Property(x => x.FormaPagoId)
               .HasColumnName("forma_pago_id").IsRequired();

        builder.Property(x => x.MonedaId)
               .HasColumnName("moneda_id").IsRequired();

        builder.Property(x => x.Habilitado)
               .HasColumnName("habilitado").HasDefaultValue(true);

        builder.HasIndex(x => new { x.CajaId, x.FormaPagoId, x.MonedaId }).IsUnique();
    }
}