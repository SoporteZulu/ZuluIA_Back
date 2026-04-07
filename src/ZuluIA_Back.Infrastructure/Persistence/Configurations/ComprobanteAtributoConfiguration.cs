using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZuluIA_Back.Domain.Entities.Comprobantes;

namespace ZuluIA_Back.Infrastructure.Persistence.Configurations;

public class ComprobanteAtributoConfiguration : IEntityTypeConfiguration<ComprobanteAtributo>
{
    public void Configure(EntityTypeBuilder<ComprobanteAtributo> builder)
    {
        builder.ToTable("comprobantes_atributos");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();
        builder.Property(x => x.ComprobanteId).HasColumnName("comprobante_id").IsRequired();
        builder.Property(x => x.Clave).HasColumnName("clave").HasMaxLength(100).IsRequired();
        builder.Property(x => x.Valor).HasColumnName("valor").HasMaxLength(500);
        builder.Property(x => x.TipoDato).HasColumnName("tipo_dato").HasMaxLength(50);
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");
        builder.Property(x => x.CreatedBy).HasColumnName("created_by");
        builder.Property(x => x.UpdatedBy).HasColumnName("updated_by");

        builder.HasIndex(x => new { x.ComprobanteId, x.Clave });
        builder.HasOne(x => x.Comprobante)
            .WithMany()
            .HasForeignKey(x => x.ComprobanteId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
