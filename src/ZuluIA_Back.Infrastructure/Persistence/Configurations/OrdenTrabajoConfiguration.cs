using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZuluIA_Back.Domain.Entities.Produccion;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Infrastructure.Persistence.Configurations;

public class OrdenTrabajoConfiguration : IEntityTypeConfiguration<OrdenTrabajo>
{
    public void Configure(EntityTypeBuilder<OrdenTrabajo> builder)
    {
        builder.ToTable("ordenes_trabajo");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();

        builder.Property(x => x.SucursalId)
               .HasColumnName("sucursal_id")
               .IsRequired();

        builder.Property(x => x.FormulaId)
               .HasColumnName("formula_id")
               .IsRequired();

        builder.Property(x => x.DepositoOrigenId)
               .HasColumnName("deposito_origen_id")
               .IsRequired();

        builder.Property(x => x.DepositoDestinoId)
               .HasColumnName("deposito_destino_id")
               .IsRequired();

        builder.Property(x => x.Fecha)
               .HasColumnName("fecha")
               .IsRequired();

        builder.Property(x => x.FechaFinPrevista)
               .HasColumnName("fecha_fin_prevista");

        builder.Property(x => x.FechaFinReal)
               .HasColumnName("fecha_fin_real");

        builder.Property(x => x.Cantidad)
               .HasColumnName("cantidad")
               .HasPrecision(18, 4)
               .IsRequired();

        builder.Property(x => x.Estado)
               .HasColumnName("estado")
               .HasConversion(
                   v => v.ToString().ToUpperInvariant(),
                   v => Enum.Parse<EstadoOrdenTrabajo>(v, true))
               .HasMaxLength(20)
               .IsRequired();

        builder.Property(x => x.Observacion)
               .HasColumnName("observacion");

        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Ignore(x => x.DeletedAt);
        builder.Property(x => x.CreatedBy).HasColumnName("created_by");
        builder.Property(x => x.UpdatedBy).HasColumnName("updated_by");

        builder.HasIndex(x => x.SucursalId);
        builder.HasIndex(x => x.FormulaId);
        builder.HasIndex(x => x.Estado);
        builder.HasIndex(x => x.Fecha);
    }
}