using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZuluIA_Back.Domain.Entities.Comprobantes;

namespace ZuluIA_Back.Infrastructure.Persistence.Configurations;

public class ComprobanteCotConfiguration : IEntityTypeConfiguration<ComprobanteCot>
{
    public void Configure(EntityTypeBuilder<ComprobanteCot> builder)
    {
        builder.ToTable("cmp_cot");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();

        builder.Property(x => x.ComprobanteId)
               .HasColumnName("cmp_id")
               .IsRequired();

        builder.Property(x => x.Numero)
               .HasColumnName("cot_valor")
               .HasMaxLength(50)
               .IsRequired();

        builder.Property(x => x.FechaVigencia)
               .HasColumnName("cot_fecha")
               .IsRequired();

        builder.Property(x => x.Descripcion)
               .HasColumnName("cot_descripcion")
               .HasMaxLength(500);

        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");
        builder.Property(x => x.CreatedBy).HasColumnName("created_by");
        builder.Property(x => x.UpdatedBy).HasColumnName("updated_by");

        builder.HasIndex(x => x.ComprobanteId).IsUnique();
        builder.HasIndex(x => x.Numero);
        builder.HasIndex(x => x.FechaVigencia);
    }
}
