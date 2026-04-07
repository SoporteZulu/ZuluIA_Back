using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZuluIA_Back.Domain.Entities.Finanzas;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Infrastructure.Persistence.Configurations;

public class ChequeHistorialConfiguration : IEntityTypeConfiguration<ChequeHistorial>
{
    public void Configure(EntityTypeBuilder<ChequeHistorial> builder)
    {
        builder.ToTable("cheques_historial");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();
        builder.Property(x => x.ChequeId).HasColumnName("cheque_id").IsRequired();
        builder.Property(x => x.CajaId).HasColumnName("caja_id").IsRequired();
        builder.Property(x => x.TerceroId).HasColumnName("tercero_id");
        builder.Property(x => x.Operacion)
            .HasColumnName("operacion")
            .HasConversion(v => v.ToString().ToUpperInvariant(), v => Enum.Parse<TipoOperacionCheque>(v, true))
            .HasMaxLength(30)
            .IsRequired();
        builder.Property(x => x.EstadoAnterior)
            .HasColumnName("estado_anterior")
            .HasConversion(v => v.HasValue ? v.Value.ToString().ToUpperInvariant() : null, v => string.IsNullOrWhiteSpace(v) ? null : Enum.Parse<EstadoCheque>(v, true))
            .HasMaxLength(20);
        builder.Property(x => x.EstadoNuevo)
            .HasColumnName("estado_nuevo")
            .HasConversion(v => v.ToString().ToUpperInvariant(), v => Enum.Parse<EstadoCheque>(v, true))
            .HasMaxLength(20)
            .IsRequired();
        builder.Property(x => x.FechaOperacion).HasColumnName("fecha_operacion").IsRequired();
        builder.Property(x => x.FechaAcreditacion).HasColumnName("fecha_acreditacion");
        builder.Property(x => x.Observacion).HasColumnName("observacion").HasMaxLength(500);
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");
        builder.Property(x => x.CreatedBy).HasColumnName("created_by");
        builder.Property(x => x.UpdatedBy).HasColumnName("updated_by");
        builder.HasIndex(x => x.ChequeId);
        builder.HasIndex(x => x.FechaOperacion);
        builder.HasIndex(x => x.Operacion);
        builder.HasIndex(x => x.EstadoNuevo);
    }
}
