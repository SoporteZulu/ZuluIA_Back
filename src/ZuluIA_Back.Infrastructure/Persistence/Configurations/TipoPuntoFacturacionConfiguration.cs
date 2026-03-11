using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZuluIA_Back.Domain.Entities.Facturacion;

namespace ZuluIA_Back.Infrastructure.Persistence.Configurations;

public class TipoPuntoFacturacionConfiguration : IEntityTypeConfiguration<TipoPuntoFacturacion>
{
    public void Configure(EntityTypeBuilder<TipoPuntoFacturacion> builder)
    {
        builder.ToTable("tipos_punto_facturacion");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();

        builder.Property(x => x.Descripcion)
               .HasColumnName("descripcion")
               .HasMaxLength(100)
               .IsRequired();

        builder.Property(x => x.PorDefecto)
               .HasColumnName("por_defecto")
               .HasDefaultValue(false);
    }
}