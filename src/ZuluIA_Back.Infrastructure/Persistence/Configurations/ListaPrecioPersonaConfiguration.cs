using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZuluIA_Back.Domain.Entities.Precios;

namespace ZuluIA_Back.Infrastructure.Persistence.Configurations;

public class ListaPrecioPersonaConfiguration : IEntityTypeConfiguration<ListaPrecioPersona>
{
    public void Configure(EntityTypeBuilder<ListaPrecioPersona> builder)
    {
        builder.ToTable("LISTASPRECIOSPERSONAS");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();

        builder.Property(x => x.ListaPreciosId).HasColumnName("id_listaprecio").IsRequired();
        builder.Property(x => x.PersonaId).HasColumnName("id_persona").IsRequired();

        builder.HasIndex(x => new { x.ListaPreciosId, x.PersonaId }).IsUnique();
        builder.HasIndex(x => x.PersonaId);
    }
}
