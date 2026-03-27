using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZuluIA_Back.Domain.Entities.Produccion;

namespace ZuluIA_Back.Infrastructure.Persistence.Configurations;

public class FormulaProduccionHistorialConfiguration : IEntityTypeConfiguration<FormulaProduccionHistorial>
{
    public void Configure(EntityTypeBuilder<FormulaProduccionHistorial> builder)
    {
        builder.ToTable("formulas_produccion_historial");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();
        builder.Property(x => x.FormulaId).HasColumnName("formula_id").IsRequired();
        builder.Property(x => x.Version).HasColumnName("version").IsRequired();
        builder.Property(x => x.Codigo).HasColumnName("codigo").HasMaxLength(50).IsRequired();
        builder.Property(x => x.Descripcion).HasColumnName("descripcion").HasMaxLength(300).IsRequired();
        builder.Property(x => x.CantidadResultado).HasColumnName("cantidad_resultado").HasPrecision(18, 4).IsRequired();
        builder.Property(x => x.Motivo).HasColumnName("motivo").HasMaxLength(200);
        builder.Property(x => x.SnapshotJson).HasColumnName("snapshot_json").IsRequired();
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");
        builder.Property(x => x.CreatedBy).HasColumnName("created_by");
        builder.Property(x => x.UpdatedBy).HasColumnName("updated_by");
        builder.HasIndex(x => new { x.FormulaId, x.Version }).IsUnique();
    }
}
