using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZuluIA_Back.Domain.Entities.Terceros;

namespace ZuluIA_Back.Infrastructure.Persistence.Configurations;

public class PersonaXTipoPersonaConfiguration : IEntityTypeConfiguration<PersonaXTipoPersona>
{
    public void Configure(EntityTypeBuilder<PersonaXTipoPersona> builder)
    {
        builder.ToTable("PER_PERSONAxTIPOPERSONA");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("pxtp_id").UseIdentityColumn();

        builder.Property(x => x.PersonaId)
               .HasColumnName("per_id").IsRequired();

        builder.Property(x => x.TipoPersonaId)
               .HasColumnName("tper_id").IsRequired();

        builder.Property(x => x.Legajo)
               .HasColumnName("pxtp_legajo").HasMaxLength(50);

        builder.Property(x => x.LegajoOrden)
               .HasColumnName("pxtp_legajoorden");

        builder.HasIndex(x => new { x.PersonaId, x.TipoPersonaId }).IsUnique();
    }
}
