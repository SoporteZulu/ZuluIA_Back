using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZuluIA_Back.Domain.Entities.Compras;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Infrastructure.Persistence.Configurations;

public class RequisicionCompraConfiguration : IEntityTypeConfiguration<RequisicionCompra>
{
    public void Configure(EntityTypeBuilder<RequisicionCompra> builder)
    {
        builder.ToTable("requisiciones_compra");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();
        builder.Property(x => x.SucursalId).HasColumnName("sucursal_id").IsRequired();
        builder.Property(x => x.SolicitanteId).HasColumnName("solicitante_id").IsRequired();
        builder.Property(x => x.Fecha).HasColumnName("fecha").IsRequired();
        builder.Property(x => x.Descripcion).HasColumnName("descripcion").HasMaxLength(300).IsRequired();
        builder.Property(x => x.Observacion).HasColumnName("observacion").HasMaxLength(500);

        builder.Property(x => x.Estado)
               .HasColumnName("estado")
               .HasConversion(v => v.ToString().ToUpperInvariant(), v => Enum.Parse<EstadoRequisicion>(v, true))
               .HasMaxLength(20).IsRequired();

        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");
        builder.Property(x => x.CreatedBy).HasColumnName("created_by");
        builder.Property(x => x.UpdatedBy).HasColumnName("updated_by");

        builder.HasMany(x => x.Items).WithOne().HasForeignKey(x => x.RequisicionId).OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(x => x.SucursalId);
    }
}

public class RequisicionCompraItemConfiguration : IEntityTypeConfiguration<RequisicionCompraItem>
{
    public void Configure(EntityTypeBuilder<RequisicionCompraItem> builder)
    {
        builder.ToTable("requisiciones_compra_items");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();
        builder.Property(x => x.RequisicionId).HasColumnName("requisicion_id").IsRequired();
        builder.Property(x => x.ItemId).HasColumnName("item_id");
        builder.Property(x => x.Descripcion).HasColumnName("descripcion").HasMaxLength(300).IsRequired();
        builder.Property(x => x.Cantidad).HasColumnName("cantidad").HasPrecision(18, 4).IsRequired();
        builder.Property(x => x.UnidadMedida).HasColumnName("unidad_medida").HasMaxLength(20).IsRequired();
        builder.Property(x => x.Observacion).HasColumnName("observacion").HasMaxLength(300);
        builder.HasIndex(x => x.RequisicionId);
    }
}
