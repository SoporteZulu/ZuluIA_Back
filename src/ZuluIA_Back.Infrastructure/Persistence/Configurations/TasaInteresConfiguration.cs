using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZuluIA_Back.Domain.Entities.Finanzas;

namespace ZuluIA_Back.Infrastructure.Persistence.Configurations;

public class TasaInteresConfiguration : IEntityTypeConfiguration<TasaInteres>
{
    public void Configure(EntityTypeBuilder<TasaInteres> builder)
    {
        builder.ToTable("tasas_interes");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).HasColumnName("id").UseIdentityColumn();
        builder.Property(t => t.Descripcion).HasColumnName("descripcion").HasMaxLength(200).IsRequired();
        builder.Property(t => t.TasaMensual).HasColumnName("tasa_mensual").HasColumnType("numeric(7,4)").IsRequired();
        builder.Property(t => t.FechaDesde).HasColumnName("fecha_desde").IsRequired();
        builder.Property(t => t.FechaHasta).HasColumnName("fecha_hasta");
        builder.Property(t => t.Activo).HasColumnName("activo").IsRequired();
        builder.Property(t => t.CreatedAt).HasColumnName("created_at");
        builder.Property(t => t.UpdatedAt).HasColumnName("updated_at");
        builder.Property(t => t.DeletedAt).HasColumnName("deleted_at");
        builder.Property(t => t.CreatedBy).HasColumnName("created_by");
        builder.Property(t => t.UpdatedBy).HasColumnName("updated_by");
    }
}
