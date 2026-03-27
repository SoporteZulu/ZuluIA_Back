using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZuluIA_Back.Domain.Entities.Finanzas;

namespace ZuluIA_Back.Infrastructure.Persistence.Configurations;

public class CierreCajaConfiguration : IEntityTypeConfiguration<CierreCaja>
{
    public void Configure(EntityTypeBuilder<CierreCaja> builder)
    {
        builder.ToTable("cierre_caja");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();

        builder.Property(x => x.FechaCierre)
               .HasColumnName("fecha_cierre").IsRequired();

        builder.Property(x => x.UsuarioId)
               .HasColumnName("usuario_id").IsRequired();

        builder.Property(x => x.FechaApertura)
               .HasColumnName("fecha_apertura").IsRequired();

        builder.Property(x => x.FechaAlta)
               .HasColumnName("fecha_alta").IsRequired();

        builder.Property(x => x.FechaControlTesoreria)
               .HasColumnName("fecha_control_tesoreria");

        builder.Property(x => x.NroCierre)
               .HasColumnName("nro_cierre").IsRequired();
    }
}
