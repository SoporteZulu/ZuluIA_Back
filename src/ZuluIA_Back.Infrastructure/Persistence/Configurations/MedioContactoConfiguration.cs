using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZuluIA_Back.Domain.Entities.Terceros;

namespace ZuluIA_Back.Infrastructure.Persistence.Configurations;

public class MedioContactoConfiguration : IEntityTypeConfiguration<MedioContacto>
{
    public void Configure(EntityTypeBuilder<MedioContacto> builder)
    {
        builder.ToTable("PER_MEDIOCONTACTO");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("mcon_id").UseIdentityColumn();

        builder.Property(x => x.PersonaId)
               .HasColumnName("per_id").IsRequired();

        builder.Property(x => x.TipoMedioContactoId)
               .HasColumnName("tmc_id");

        builder.Property(x => x.Valor)
               .HasColumnName("mcon_valor").HasMaxLength(200).IsRequired();

        builder.Property(x => x.Orden)
               .HasColumnName("mcon_orden").IsRequired();

        builder.Property(x => x.EsDefecto)
               .HasColumnName("mcon_defecto").IsRequired();

        builder.Property(x => x.Observacion)
               .HasColumnName("mcon_observacion").HasMaxLength(500);

        builder.HasIndex(x => x.PersonaId);
    }
}
