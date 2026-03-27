using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZuluIA_Back.Domain.Entities.Finanzas;

namespace ZuluIA_Back.Infrastructure.Persistence.Configurations;

public class RetencionXPersonaConfiguration : IEntityTypeConfiguration<RetencionXPersona>
{
    public void Configure(EntityTypeBuilder<RetencionXPersona> builder)
    {
        builder.ToTable("retenciones_x_persona");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();

        builder.Property(x => x.TerceroId)
               .HasColumnName("tercero_id")
               .IsRequired();

        builder.Property(x => x.TipoRetencionId)
               .HasColumnName("tipo_retencion_id")
               .IsRequired();

        builder.Property(x => x.Descripcion)
               .HasColumnName("descripcion")
               .HasMaxLength(200);

        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");

        builder.HasIndex(x => new { x.TerceroId, x.TipoRetencionId }).IsUnique();
    }
}
