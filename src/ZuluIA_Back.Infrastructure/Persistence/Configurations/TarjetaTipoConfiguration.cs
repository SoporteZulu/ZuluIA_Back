using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZuluIA_Back.Domain.Entities.Finanzas;

namespace ZuluIA_Back.Infrastructure.Persistence.Configurations;

public class TarjetaTipoConfiguration : IEntityTypeConfiguration<TarjetaTipo>
{
    public void Configure(EntityTypeBuilder<TarjetaTipo> builder)
    {
        builder.ToTable("tarjetas_tipos");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();
        builder.Property(x => x.Codigo).HasColumnName("codigo").HasMaxLength(20).IsRequired();
        builder.Property(x => x.Descripcion).HasColumnName("descripcion").HasMaxLength(100).IsRequired();
        builder.Property(x => x.EsDebito).HasColumnName("es_debito").HasDefaultValue(false);
        builder.Property(x => x.Activa).HasColumnName("activa").HasDefaultValue(true);
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");
        builder.Property(x => x.CreatedBy).HasColumnName("created_by");
        builder.Property(x => x.UpdatedBy).HasColumnName("updated_by");
        builder.HasIndex(x => x.Codigo).IsUnique();
    }
}
