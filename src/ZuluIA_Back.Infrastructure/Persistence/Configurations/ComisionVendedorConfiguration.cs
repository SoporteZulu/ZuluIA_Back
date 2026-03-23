using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZuluIA_Back.Domain.Entities.Ventas;

namespace ZuluIA_Back.Infrastructure.Persistence.Configurations;

public class ComisionVendedorConfiguration : IEntityTypeConfiguration<ComisionVendedor>
{
    public void Configure(EntityTypeBuilder<ComisionVendedor> builder)
    {
        builder.ToTable("comisiones_vendedor");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).HasColumnName("id").UseIdentityColumn();
        builder.Property(c => c.SucursalId).HasColumnName("sucursal_id").IsRequired();
        builder.Property(c => c.VendedorId).HasColumnName("vendedor_id").IsRequired();
        builder.Property(c => c.Periodo).HasColumnName("periodo").IsRequired();
        builder.Property(c => c.MontoBase).HasColumnName("monto_base").HasColumnType("numeric(18,2)").IsRequired();
        builder.Property(c => c.PorcentajeComision).HasColumnName("porcentaje_comision").HasColumnType("numeric(5,2)").IsRequired();
        builder.Property(c => c.MontoComision).HasColumnName("monto_comision").HasColumnType("numeric(18,2)").IsRequired();
        builder.Property(c => c.Estado).HasColumnName("estado")
            .HasConversion(v => v.ToString().ToUpperInvariant(), v => Enum.Parse<Domain.Enums.EstadoComision>(v, true))
            .HasMaxLength(20).IsRequired();
        builder.Property(c => c.Observacion).HasColumnName("observacion").HasMaxLength(500);
        builder.Property(c => c.CreatedAt).HasColumnName("created_at");
        builder.Property(c => c.UpdatedAt).HasColumnName("updated_at");
        builder.Property(c => c.DeletedAt).HasColumnName("deleted_at");
        builder.Property(c => c.CreatedBy).HasColumnName("created_by");
        builder.Property(c => c.UpdatedBy).HasColumnName("updated_by");
    }
}
