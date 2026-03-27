using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZuluIA_Back.Domain.Entities.Facturacion;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Infrastructure.Persistence.Configurations;

public class CartaPorteEventoConfiguration : IEntityTypeConfiguration<CartaPorteEvento>
{
    public void Configure(EntityTypeBuilder<CartaPorteEvento> builder)
    {
        builder.ToTable("carta_porte_eventos");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();
        builder.Property(x => x.CartaPorteId).HasColumnName("carta_porte_id").IsRequired();
        builder.Property(x => x.OrdenCargaId).HasColumnName("orden_carga_id");
        builder.Property(x => x.TipoEvento)
            .HasColumnName("tipo_evento")
            .HasConversion(v => v.ToString().ToUpperInvariant(), v => Enum.Parse<TipoEventoCartaPorte>(v, true))
            .HasMaxLength(30)
            .IsRequired();
        builder.Property(x => x.EstadoAnterior)
            .HasColumnName("estado_anterior")
            .HasConversion(v => v.HasValue ? v.Value.ToString().ToUpperInvariant() : null, v => string.IsNullOrWhiteSpace(v) ? null : Enum.Parse<EstadoCartaPorte>(v, true))
            .HasMaxLength(30);
        builder.Property(x => x.EstadoNuevo)
            .HasColumnName("estado_nuevo")
            .HasConversion(v => v.ToString().ToUpperInvariant(), v => Enum.Parse<EstadoCartaPorte>(v, true))
            .HasMaxLength(30)
            .IsRequired();
        builder.Property(x => x.FechaEvento).HasColumnName("fecha_evento").IsRequired();
        builder.Property(x => x.Mensaje).HasColumnName("mensaje").HasMaxLength(1000);
        builder.Property(x => x.NroCtg).HasColumnName("nro_ctg").HasMaxLength(30);
        builder.Property(x => x.IntentoCtg).HasColumnName("intento_ctg").IsRequired();
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");
        builder.Property(x => x.CreatedBy).HasColumnName("created_by");
        builder.Property(x => x.UpdatedBy).HasColumnName("updated_by");
        builder.HasIndex(x => x.CartaPorteId);
        builder.HasIndex(x => x.TipoEvento);
        builder.HasIndex(x => x.FechaEvento);
    }
}
