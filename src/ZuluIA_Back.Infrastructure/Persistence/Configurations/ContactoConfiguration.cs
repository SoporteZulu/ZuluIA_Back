using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZuluIA_Back.Domain.Entities.Terceros;

namespace ZuluIA_Back.Infrastructure.Persistence.Configurations;

public class ContactoConfiguration : IEntityTypeConfiguration<Contacto>
{
    public void Configure(EntityTypeBuilder<Contacto> builder)
    {
        builder.ToTable("CONTACTOS");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();

        builder.Property(x => x.PersonaId)
               .HasColumnName("id_persona").IsRequired();

        builder.Property(x => x.PersonaContactoId)
               .HasColumnName("id_contacto").IsRequired();

        builder.Property(x => x.TipoRelacionId)
               .HasColumnName("trel_id");

        builder.HasIndex(x => x.PersonaId);
        builder.HasIndex(x => new { x.PersonaId, x.PersonaContactoId });
    }
}
