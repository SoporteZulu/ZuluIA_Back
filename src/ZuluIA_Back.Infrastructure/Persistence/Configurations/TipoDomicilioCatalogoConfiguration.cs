using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZuluIA_Back.Domain.Entities.Terceros;

namespace ZuluIA_Back.Infrastructure.Persistence.Configurations;

public class TipoDomicilioCatalogoConfiguration : IEntityTypeConfiguration<TipoDomicilioCatalogo>
{
    public void Configure(EntityTypeBuilder<TipoDomicilioCatalogo> builder)
    {
        builder.ToTable("tipodomicilio");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();
        builder.Property(x => x.Descripcion).HasColumnName("tipodomicilio").HasMaxLength(100).IsRequired();
    }
}
