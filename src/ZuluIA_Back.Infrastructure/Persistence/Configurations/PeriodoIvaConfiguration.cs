using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZuluIA_Back.Domain.Entities.Facturacion;

namespace ZuluIA_Back.Infrastructure.Persistence.Configurations;

public class PeriodoIvaConfiguration : IEntityTypeConfiguration<PeriodoIva>
{
    public void Configure(EntityTypeBuilder<PeriodoIva> builder)
    {
        builder.ToTable("periodos_iva");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();

        builder.Property(x => x.EjercicioId)
               .HasColumnName("ejercicio_id")
               .IsRequired();

        builder.Property(x => x.SucursalId)
               .HasColumnName("sucursal_id")
               .IsRequired();

        builder.Property(x => x.Periodo)
               .HasColumnName("periodo")
               .IsRequired();

        builder.Property(x => x.Cerrado)
               .HasColumnName("cerrado")
               .HasDefaultValue(false);

        builder.Property(x => x.CreatedAt)
               .HasColumnName("created_at");

        // Ignorar propiedad calculada
        builder.Ignore(x => x.PeriodoDescripcion);

        builder.HasIndex(x => new { x.SucursalId, x.Periodo }).IsUnique();
        builder.HasIndex(x => x.EjercicioId);
    }
}