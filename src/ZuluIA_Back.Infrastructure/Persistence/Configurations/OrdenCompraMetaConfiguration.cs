using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZuluIA_Back.Domain.Entities.Comprobantes;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Infrastructure.Persistence.Configurations;

public class OrdenCompraMetaConfiguration : IEntityTypeConfiguration<OrdenCompraMeta>
{
    public void Configure(EntityTypeBuilder<OrdenCompraMeta> builder)
    {
        builder.ToTable("ordenes_compra_meta");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();

        builder.Property(x => x.ComprobanteId)
               .HasColumnName("comprobante_id").IsRequired();

        builder.Property(x => x.ProveedorId)
               .HasColumnName("proveedor_id").IsRequired();

        builder.Property(x => x.FechaEntregaReq)
               .HasColumnName("fecha_entrega_req");

        builder.Property(x => x.CondicionesEntrega)
               .HasColumnName("condiciones_entrega");

        builder.Property(x => x.EstadoOc)
               .HasColumnName("estado_oc")
               .HasConversion(
                   v => v.ToString().ToUpperInvariant(),
                   v => Enum.Parse<EstadoOrdenCompra>(v, true))
               .HasMaxLength(20).IsRequired();

        builder.Property(x => x.Habilitada)
               .HasColumnName("habilitada")
               .HasDefaultValue(true);

        builder.Property(x => x.CreatedAt)
               .HasColumnName("created_at");

        builder.HasIndex(x => x.ComprobanteId).IsUnique();
        builder.HasIndex(x => x.ProveedorId);
        builder.HasIndex(x => x.EstadoOc);
    }
}