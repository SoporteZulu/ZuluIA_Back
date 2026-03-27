using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZuluIA_Back.Domain.Entities.Produccion;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Infrastructure.Persistence.Configurations;

public class OrdenEmpaqueConfiguration : IEntityTypeConfiguration<OrdenEmpaque>
{
    public void Configure(EntityTypeBuilder<OrdenEmpaque> builder)
    {
        builder.ToTable("ordenes_empaque");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();
        builder.Property(x => x.OrdenTrabajoId).HasColumnName("orden_trabajo_id").IsRequired();
        builder.Property(x => x.ItemId).HasColumnName("item_id").IsRequired();
        builder.Property(x => x.DepositoId).HasColumnName("deposito_id").IsRequired();
        builder.Property(x => x.Fecha).HasColumnName("fecha").IsRequired();
        builder.Property(x => x.Cantidad).HasColumnName("cantidad").HasPrecision(18, 4).IsRequired();
        builder.Property(x => x.Lote).HasColumnName("lote").HasMaxLength(100);
        builder.Property(x => x.Observacion).HasColumnName("observacion").HasMaxLength(500);
        builder.Property(x => x.Estado)
            .HasColumnName("estado")
            .HasConversion(v => v.ToString().ToUpperInvariant(), v => Enum.Parse<EstadoOrdenEmpaque>(v, true))
            .HasMaxLength(20)
            .IsRequired();
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");
        builder.Property(x => x.CreatedBy).HasColumnName("created_by");
        builder.Property(x => x.UpdatedBy).HasColumnName("updated_by");
        builder.HasIndex(x => x.OrdenTrabajoId);
        builder.HasIndex(x => x.Estado);
        builder.HasIndex(x => x.Fecha);
    }
}
