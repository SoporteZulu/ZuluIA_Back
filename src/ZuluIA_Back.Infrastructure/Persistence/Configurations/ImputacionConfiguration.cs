using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZuluIA_Back.Domain.Entities.Comprobantes;

namespace ZuluIA_Back.Infrastructure.Persistence.Configurations;

public class ImputacionConfiguration : IEntityTypeConfiguration<Imputacion>
{
    public void Configure(EntityTypeBuilder<Imputacion> builder)
    {
        builder.ToTable("imputaciones");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();

        builder.Property(x => x.ComprobanteOrigenId)
               .HasColumnName("comprobante_origen_id").IsRequired();

        builder.Property(x => x.ComprobanteDestinoId)
               .HasColumnName("comprobante_destino_id").IsRequired();

        builder.Property(x => x.Importe)
               .HasColumnName("importe")
               .HasPrecision(18, 4).IsRequired();

        builder.Property(x => x.Fecha)
               .HasColumnName("fecha").IsRequired();

        builder.Property(x => x.Anulada)
               .HasColumnName("anulada")
               .HasDefaultValue(false)
               .IsRequired();

        builder.Property(x => x.FechaDesimputacion)
               .HasColumnName("fecha_desimputacion");

        builder.Property(x => x.MotivoDesimputacion)
               .HasColumnName("motivo_desimputacion")
               .HasMaxLength(500);

        builder.Property(x => x.DesimputadaAt)
               .HasColumnName("desimputada_at");

        builder.Property(x => x.DesimputadaBy)
               .HasColumnName("desimputada_by");

        builder.Property(x => x.CreatedAt)
               .HasColumnName("created_at");

        builder.Property(x => x.CreatedBy)
               .HasColumnName("created_by");

        builder.HasIndex(x => x.ComprobanteOrigenId);
        builder.HasIndex(x => x.ComprobanteDestinoId);
        builder.HasIndex(x => x.Fecha);
        builder.HasIndex(x => x.Anulada);
    }
}