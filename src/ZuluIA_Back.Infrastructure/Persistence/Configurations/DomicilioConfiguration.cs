using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZuluIA_Back.Domain.Entities.Terceros;

namespace ZuluIA_Back.Infrastructure.Persistence.Configurations;

public class DomicilioConfiguration : IEntityTypeConfiguration<PersonaDomicilio>
{
    public void Configure(EntityTypeBuilder<PersonaDomicilio> builder)
    {
        builder.ToTable("PER_DOMICILIO");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("dom_id").UseIdentityColumn();

        builder.Property(x => x.PersonaId)
               .HasColumnName("per_id").IsRequired();

        builder.Property(x => x.TipoDomicilioId)
               .HasColumnName("tdom_id");

        builder.Property(x => x.ProvinciaId)
               .HasColumnName("prov_id");

        builder.Property(x => x.LocalidadId)
               .HasColumnName("loc_id");

        builder.Property(x => x.Calle)
               .HasColumnName("dom_domicilio").HasMaxLength(200);

        builder.Property(x => x.Barrio)
               .HasColumnName("dom_barrio").HasMaxLength(100);

        builder.Property(x => x.CodigoPostal)
               .HasColumnName("dom_cp").HasMaxLength(20);

        builder.Property(x => x.Observacion)
               .HasColumnName("dom_observacion").HasMaxLength(500);

        builder.Property(x => x.Orden)
               .HasColumnName("dom_orden").IsRequired();

        builder.Property(x => x.EsDefecto)
               .HasColumnName("dom_defecto").IsRequired();

        builder.HasIndex(x => x.PersonaId);
    }
}
