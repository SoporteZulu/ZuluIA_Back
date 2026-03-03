using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZuluIA_Back.Domain.Entities.Facturacion;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Infrastructure.Persistence.Configurations;

public class CartaPorteConfiguration : IEntityTypeConfiguration<CartaPorte>
{
    public void Configure(EntityTypeBuilder<CartaPorte> builder)
    {
        builder.ToTable("carta_porte");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();

        builder.Property(x => x.ComprobanteId)
               .HasColumnName("comprobante_id");

        builder.Property(x => x.NroCtg)
               .HasColumnName("nro_ctg")
               .HasMaxLength(30);

        builder.Property(x => x.CuitRemitente)
               .HasColumnName("cuit_remitente")
               .HasMaxLength(20)
               .IsRequired();

        builder.Property(x => x.CuitDestinatario)
               .HasColumnName("cuit_destinatario")
               .HasMaxLength(20)
               .IsRequired();

        builder.Property(x => x.CuitTransportista)
               .HasColumnName("cuit_transportista")
               .HasMaxLength(20);

        builder.Property(x => x.FechaEmision)
               .HasColumnName("fecha_emision")
               .IsRequired();

        builder.Property(x => x.Estado)
               .HasColumnName("estado")
               .HasConversion(
                   v => v.ToString().ToUpperInvariant(),
                   v => Enum.Parse<EstadoCartaPorte>(v, true))
               .HasMaxLength(20)
               .IsRequired();

        builder.Property(x => x.Observacion)
               .HasColumnName("observacion");

        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");
        builder.Property(x => x.CreatedBy).HasColumnName("created_by");
        builder.Property(x => x.UpdatedBy).HasColumnName("updated_by");

        builder.HasIndex(x => x.NroCtg);
        builder.HasIndex(x => x.ComprobanteId);
        builder.HasIndex(x => x.Estado);
        builder.HasIndex(x => x.FechaEmision);
    }
}