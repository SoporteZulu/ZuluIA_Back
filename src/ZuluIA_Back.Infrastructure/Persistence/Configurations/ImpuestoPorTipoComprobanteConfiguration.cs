using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZuluIA_Back.Domain.Entities.Impuestos;

namespace ZuluIA_Back.Infrastructure.Persistence.Configurations;

public class ImpuestoPorTipoComprobanteConfiguration
    : IEntityTypeConfiguration<ImpuestoPorTipoComprobante>
{
    public void Configure(EntityTypeBuilder<ImpuestoPorTipoComprobante> builder)
    {
        builder.ToTable("IMP_IMPUESTOXTIPOCOMPROBANTE");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id");

        builder.Property(e => e.ImpuestoId)
               .IsRequired()
               .HasColumnName("imp_id");

        builder.Property(e => e.TipoComprobanteId)
               .IsRequired()
               .HasColumnName("tc_id");

        builder.Property(e => e.Orden)
               .HasColumnName("ixtc_orden");

        builder.HasIndex(e => new { e.ImpuestoId, e.TipoComprobanteId })
               .IsUnique();
    }
}
