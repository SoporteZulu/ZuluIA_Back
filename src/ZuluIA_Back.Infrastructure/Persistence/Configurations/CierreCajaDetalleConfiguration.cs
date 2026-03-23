using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZuluIA_Back.Domain.Entities.Finanzas;

namespace ZuluIA_Back.Infrastructure.Persistence.Configurations;

public class CierreCajaDetalleConfiguration : IEntityTypeConfiguration<CierreCajaDetalle>
{
    public void Configure(EntityTypeBuilder<CierreCajaDetalle> builder)
    {
        builder.ToTable("cierre_caja_detalle");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();

        builder.Property(x => x.CierreId)
               .HasColumnName("cierre_id").IsRequired();

        builder.Property(x => x.CajaCuentaBancariaId)
               .HasColumnName("caja_cuenta_bancaria_id").IsRequired();

        builder.Property(x => x.Diferencia)
               .HasColumnName("diferencia")
               .HasColumnType("numeric(18,4)")
               .IsRequired();
    }
}
