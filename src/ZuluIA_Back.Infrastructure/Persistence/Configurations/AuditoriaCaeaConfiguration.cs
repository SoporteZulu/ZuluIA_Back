using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZuluIA_Back.Domain.Entities.Auditoria;

namespace ZuluIA_Back.Infrastructure.Persistence.Configurations;

public class AuditoriaCaeaConfiguration : IEntityTypeConfiguration<AuditoriaCaea>
{
    public void Configure(EntityTypeBuilder<AuditoriaCaea> builder)
    {
        builder.ToTable("auditoria_caeas");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).HasColumnName("id").UseIdentityColumn();
        builder.Property(a => a.CaeaId).HasColumnName("caea_id").IsRequired();
        builder.Property(a => a.UsuarioId).HasColumnName("usuario_id");
        builder.Property(a => a.Accion).HasColumnName("accion")
            .HasConversion(v => v.ToString().ToUpperInvariant(), v => Enum.Parse<Domain.Enums.AccionAuditoria>(v, true))
            .HasMaxLength(30).IsRequired();
        builder.Property(a => a.FechaHora).HasColumnName("fecha_hora").IsRequired();
        builder.Property(a => a.DetalleCambio).HasColumnName("detalle_cambio").HasMaxLength(2000);
        builder.Property(a => a.IpOrigen).HasColumnName("ip_origen").HasMaxLength(45);
    }
}