using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZuluIA_Back.Domain.Entities.Terceros;

namespace ZuluIA_Back.Infrastructure.Persistence.Configurations;

public class VinculacionPersonaConfiguration : IEntityTypeConfiguration<VinculacionPersona>
{
    public void Configure(EntityTypeBuilder<VinculacionPersona> builder)
    {
        builder.ToTable("PER_VINCULACIONPERSONA");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("vincp_id").UseIdentityColumn();

        builder.Property(x => x.ClienteId)
               .HasColumnName("cli_id").IsRequired();

        builder.Property(x => x.ProveedorId)
               .HasColumnName("prov_id").IsRequired();

        builder.Property(x => x.EsPredeterminado)
               .HasColumnName("vinc_default").IsRequired();

        builder.Property(x => x.TipoRelacionId)
               .HasColumnName("trel_id");

        builder.HasIndex(x => new { x.ClienteId, x.ProveedorId });
    }
}
