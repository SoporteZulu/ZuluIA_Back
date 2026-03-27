using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZuluIA_Back.Domain.Entities.Finanzas;

namespace ZuluIA_Back.Infrastructure.Persistence.Configurations;

public class TipoRetencionConfiguration : IEntityTypeConfiguration<TipoRetencion>
{
    public void Configure(EntityTypeBuilder<TipoRetencion> builder)
    {
        builder.ToTable("tipos_retencion");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();

        builder.Property(x => x.Descripcion)
               .HasColumnName("descripcion")
               .HasMaxLength(200)
               .IsRequired();

        builder.Property(x => x.Regimen)
               .HasColumnName("regimen")
               .HasMaxLength(100)
               .IsRequired();

        builder.Property(x => x.MinimoNoImponible)
               .HasColumnName("minimo_no_imponible")
               .HasPrecision(18, 4)
               .IsRequired();

        builder.Property(x => x.AcumulaPago)
               .HasColumnName("acumula_pago")
               .HasDefaultValue(false);

        builder.Property(x => x.TipoComprobanteId)
               .HasColumnName("tipo_comprobante_id");

        builder.Property(x => x.ItemId)
               .HasColumnName("item_id");

        builder.Property(x => x.Activo)
               .HasColumnName("activo")
               .HasDefaultValue(true);

        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");

        builder.HasMany(x => x.Escalas)
               .WithOne()
               .HasForeignKey(x => x.TipoRetencionId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
