using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZuluIA_Back.Domain.Entities.Extras;

namespace ZuluIA_Back.Infrastructure.Persistence.Configurations;

public class SorteoListaXClienteConfiguration : IEntityTypeConfiguration<SorteoListaXCliente>
{
    public void Configure(EntityTypeBuilder<SorteoListaXCliente> builder)
    {
        builder.ToTable("SORTEOLISTAXCLIENTE");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();

        builder.Property(x => x.SorteoListaId).HasColumnName("sorteo_lista_id").IsRequired();
        builder.Property(x => x.TerceroId).HasColumnName("tercero_id").IsRequired();
        builder.Property(x => x.NroTicket).HasColumnName("nro_ticket").IsRequired();
        builder.Property(x => x.Ganador).HasColumnName("ganador").HasDefaultValue(false);

        builder.HasIndex(x => new { x.SorteoListaId, x.NroTicket }).IsUnique();
        builder.HasIndex(x => new { x.SorteoListaId, x.TerceroId });
    }
}
