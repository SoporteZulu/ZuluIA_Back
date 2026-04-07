using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZuluIA_Back.Domain.Entities.Sucursales;

namespace ZuluIA_Back.Infrastructure.Persistence.Configurations;

public class SucursalDomicilioConfiguration : IEntityTypeConfiguration<SucursalDomicilio>
{
    public void Configure(EntityTypeBuilder<SucursalDomicilio> builder)
    {
        builder.ToTable("SUC_DOMICILIO");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("sdom_id").UseIdentityColumn();

        builder.Property(x => x.SucursalId)
               .HasColumnName("suc_id").IsRequired();

        builder.Property(x => x.TipoDomicilioId)
               .HasColumnName("tdom_id");

        builder.Property(x => x.ProvinciaId)
               .HasColumnName("prov_id");

        builder.Property(x => x.LocalidadId)
               .HasColumnName("loc_id");

        builder.Property(x => x.Calle)
               .HasColumnName("sdom_domicilio").HasMaxLength(200);

        builder.Property(x => x.Barrio)
               .HasColumnName("sdom_barrio").HasMaxLength(100);

        builder.Property(x => x.CodigoPostal)
               .HasColumnName("sdom_cp").HasMaxLength(20);

        builder.Property(x => x.Observacion)
               .HasColumnName("sdom_observacion").HasMaxLength(500);

        builder.Property(x => x.Orden)
               .HasColumnName("sdom_orden").IsRequired();

        builder.Property(x => x.EsDefecto)
               .HasColumnName("sdom_defecto").IsRequired();

        builder.HasIndex(x => x.SucursalId);
    }
}
