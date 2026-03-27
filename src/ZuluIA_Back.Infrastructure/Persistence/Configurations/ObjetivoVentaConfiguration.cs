using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZuluIA_Back.Domain.Entities.Ventas;

namespace ZuluIA_Back.Infrastructure.Persistence.Configurations;

public class ObjetivoVentaConfiguration : IEntityTypeConfiguration<ObjetivoVenta>
{
    public void Configure(EntityTypeBuilder<ObjetivoVenta> builder)
    {
        builder.ToTable("objetivos_venta");
        builder.HasKey(o => o.Id);
        builder.Property(o => o.Id).HasColumnName("id").UseIdentityColumn();
        builder.Property(o => o.SucursalId).HasColumnName("sucursal_id").IsRequired();
        builder.Property(o => o.VendedorId).HasColumnName("vendedor_id").IsRequired();
        builder.Property(o => o.Periodo).HasColumnName("periodo").IsRequired();
        builder.Property(o => o.MontoObjetivo).HasColumnName("monto_objetivo").HasColumnType("numeric(18,2)").IsRequired();
        builder.Property(o => o.MontoRealizado).HasColumnName("monto_realizado").HasColumnType("numeric(18,2)").IsRequired();
        builder.Property(o => o.Descripcion).HasColumnName("descripcion").HasMaxLength(500);
        builder.Property(o => o.Cerrado).HasColumnName("cerrado").IsRequired();
        builder.Property(o => o.CreatedAt).HasColumnName("created_at");
        builder.Property(o => o.UpdatedAt).HasColumnName("updated_at");
        builder.Property(o => o.DeletedAt).HasColumnName("deleted_at");
        builder.Property(o => o.CreatedBy).HasColumnName("created_by");
        builder.Property(o => o.UpdatedBy).HasColumnName("updated_by");
    }
}
