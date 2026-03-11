using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZuluIA_Back.Domain.Entities.Ventas;

namespace ZuluIA_Back.Infrastructure.Persistence.Configurations;

public class PlanPagoConfiguration : IEntityTypeConfiguration<PlanPago>
{
    public void Configure(EntityTypeBuilder<PlanPago> builder)
    {
        builder.ToTable("planes_pago");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();

        builder.Property(x => x.Descripcion)
               .HasColumnName("descripcion")
               .HasMaxLength(200)
               .IsRequired();

        builder.Property(x => x.CantidadCuotas)
               .HasColumnName("cantidad_cuotas")
               .HasDefaultValue((short)1);

        builder.Property(x => x.InteresPct)
               .HasColumnName("interes_pct")
               .HasPrecision(5, 2)
               .HasDefaultValue(0m);

        builder.Property(x => x.Activo)
               .HasColumnName("activo")
               .HasDefaultValue(true);

        builder.Property(x => x.CreatedAt)
               .HasColumnName("created_at");

        // Ignorar métodos calculados
        builder.Ignore("CalcularCuota");
        builder.Ignore("CalcularTotalConInteres");

        builder.HasIndex(x => x.Activo);
    }
}