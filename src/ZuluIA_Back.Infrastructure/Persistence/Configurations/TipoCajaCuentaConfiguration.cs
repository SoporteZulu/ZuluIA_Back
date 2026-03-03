using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZuluIA_Back.Domain.Entities.Finanzas;

namespace ZuluIA_Back.Infrastructure.Persistence.Configurations;

public class TipoCajaCuentaConfiguration : IEntityTypeConfiguration<TipoCajaCuenta>
{
    public void Configure(EntityTypeBuilder<TipoCajaCuenta> builder)
    {
        builder.ToTable("tipos_caja_cuenta");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();

        builder.Property(x => x.Descripcion)
               .HasColumnName("descripcion").HasMaxLength(100).IsRequired();

        builder.Property(x => x.EsCaja)
               .HasColumnName("es_caja").HasDefaultValue(true);
    }
}