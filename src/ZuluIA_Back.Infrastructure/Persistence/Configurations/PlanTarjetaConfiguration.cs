using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZuluIA_Back.Domain.Entities.Finanzas;

namespace ZuluIA_Back.Infrastructure.Persistence.Configurations;

public class PlanTarjetaConfiguration : IEntityTypeConfiguration<PlanTarjeta>
{
    public void Configure(EntityTypeBuilder<PlanTarjeta> builder)
    {
        builder.ToTable("planes_tarjeta");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();
        builder.Property(x => x.TarjetaTipoId).HasColumnName("tarjeta_tipo_id").IsRequired();
        builder.Property(x => x.Codigo).HasColumnName("codigo").HasMaxLength(20).IsRequired();
        builder.Property(x => x.Descripcion).HasColumnName("descripcion").HasMaxLength(100).IsRequired();
        builder.Property(x => x.CantidadCuotas).HasColumnName("cantidad_cuotas").IsRequired();
        builder.Property(x => x.Recargo).HasColumnName("recargo").HasPrecision(8, 4).IsRequired();
        builder.Property(x => x.DiasAcreditacion).HasColumnName("dias_acreditacion").IsRequired();
        builder.Property(x => x.Activo).HasColumnName("activo").HasDefaultValue(true);
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");
        builder.Property(x => x.CreatedBy).HasColumnName("created_by");
        builder.Property(x => x.UpdatedBy).HasColumnName("updated_by");
        builder.HasIndex(x => x.TarjetaTipoId);
        builder.HasIndex(x => new { x.TarjetaTipoId, x.Codigo }).IsUnique();
    }
}
