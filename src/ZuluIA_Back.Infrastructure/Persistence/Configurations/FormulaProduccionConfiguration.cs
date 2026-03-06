using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZuluIA_Back.Domain.Entities.Produccion;

namespace ZuluIA_Back.Infrastructure.Persistence.Configurations;

public class FormulaProduccionConfiguration : IEntityTypeConfiguration<FormulaProduccion>
{
    public void Configure(EntityTypeBuilder<FormulaProduccion> builder)
    {
        builder.ToTable("formulas_produccion");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();

        builder.Property(x => x.Codigo)
               .HasColumnName("codigo")
               .HasMaxLength(50)
               .IsRequired();

        builder.Property(x => x.Descripcion)
               .HasColumnName("descripcion")
               .HasMaxLength(300)
               .IsRequired();

        builder.Property(x => x.ItemResultadoId)
               .HasColumnName("item_resultado_id")
               .IsRequired();

        builder.Property(x => x.CantidadResultado)
               .HasColumnName("cantidad_resultado")
               .HasPrecision(18, 4)
               .IsRequired();

        builder.Property(x => x.UnidadMedidaId)
               .HasColumnName("unidad_medida_id");

        builder.Property(x => x.Activo)
               .HasColumnName("activo")
               .HasDefaultValue(true);

        builder.Property(x => x.Observacion)
               .HasColumnName("observacion");

        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");
        builder.Property(x => x.CreatedBy).HasColumnName("created_by");
        builder.Property(x => x.UpdatedBy).HasColumnName("updated_by");

        builder.HasMany(x => x.Ingredientes)
               .WithOne()
               .HasForeignKey(x => x.FormulaId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.Codigo).IsUnique();
        builder.HasIndex(x => x.ItemResultadoId);
        builder.HasIndex(x => x.Activo);
    }
}

public class FormulaIngredienteConfiguration : IEntityTypeConfiguration<FormulaIngrediente>
{
    public void Configure(EntityTypeBuilder<FormulaIngrediente> builder)
    {
        builder.ToTable("formula_ingredientes");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();

        builder.Property(x => x.FormulaId)
               .HasColumnName("formula_id")
               .IsRequired();

        builder.Property(x => x.ItemId)
               .HasColumnName("item_id")
               .IsRequired();

        builder.Property(x => x.Cantidad)
               .HasColumnName("cantidad")
               .HasPrecision(18, 4)
               .IsRequired();

        builder.Property(x => x.UnidadMedidaId)
               .HasColumnName("unidad_medida_id");

        builder.Property(x => x.EsOpcional)
               .HasColumnName("es_opcional")
               .HasDefaultValue(false);

        builder.Property(x => x.Orden)
               .HasColumnName("orden")
               .HasDefaultValue((short)0);

        builder.HasIndex(x => new { x.FormulaId, x.ItemId }).IsUnique();
        builder.HasIndex(x => x.FormulaId);
    }
}