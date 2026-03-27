using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZuluIA_Back.Domain.Entities.BI;

namespace ZuluIA_Back.Infrastructure.Persistence.Configurations;

public class CuboFiltroConfiguration : IEntityTypeConfiguration<CuboFiltro>
{
    public void Configure(EntityTypeBuilder<CuboFiltro> builder)
    {
        builder.ToTable("BI_CUBOFILTROS");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("cfil_id").UseIdentityColumn();

        builder.Property(x => x.CuboId)
               .HasColumnName("cub_id").IsRequired();

        builder.Property(x => x.Filtro)
               .HasColumnName("cfil_filtro").HasMaxLength(2000).IsRequired();

        builder.Property(x => x.Operador)
               .HasColumnName("cfil_operador").IsRequired();

        builder.Property(x => x.Orden)
               .HasColumnName("cfil_orden").IsRequired();

        builder.HasIndex(x => x.CuboId);
    }
}
