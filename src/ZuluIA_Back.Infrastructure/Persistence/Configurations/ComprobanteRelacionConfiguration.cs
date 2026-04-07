using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZuluIA_Back.Domain.Entities.Comprobantes;

namespace ZuluIA_Back.Infrastructure.Persistence.Configurations;

public class ComprobanteRelacionConfiguration : IEntityTypeConfiguration<ComprobanteRelacion>
{
    public void Configure(EntityTypeBuilder<ComprobanteRelacion> builder)
    {
        builder.ToTable("comprobantes_relaciones");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedOnAdd();

        builder.Property(x => x.ComprobanteOrigenId)
               .HasColumnName("comprobante_origen_id").IsRequired();

        builder.Property(x => x.ComprobanteDestinoId)
               .HasColumnName("comprobante_destino_id").IsRequired();

        builder.Property(x => x.Observacion)
               .HasColumnName("observacion")
               .HasMaxLength(500);

        builder.Property(x => x.CreatedAt)
               .HasColumnName("created_at")
               .HasDefaultValueSql("now()");

        builder.HasOne<Comprobante>()
               .WithMany()
               .HasForeignKey(x => x.ComprobanteOrigenId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Comprobante>()
               .WithMany()
               .HasForeignKey(x => x.ComprobanteDestinoId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
