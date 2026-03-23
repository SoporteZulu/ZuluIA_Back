using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZuluIA_Back.Domain.Entities.Extras;

namespace ZuluIA_Back.Infrastructure.Persistence.Configurations;

public class IntegradoraConfiguration : IEntityTypeConfiguration<Integradora>
{
    public void Configure(EntityTypeBuilder<Integradora> builder)
    {
        builder.ToTable("INTEGRADORA");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();

        builder.Property(x => x.Codigo).HasColumnName("codigo").HasMaxLength(50).IsRequired();
        builder.Property(x => x.Nombre).HasColumnName("nombre").HasMaxLength(200).IsRequired();
        builder.Property(x => x.TipoSistema).HasColumnName("tipo_sistema").HasMaxLength(50).IsRequired();
        builder.Property(x => x.UrlEndpoint).HasColumnName("url_endpoint").HasMaxLength(500);
        builder.Property(x => x.ApiKey).HasColumnName("api_key").HasMaxLength(500);
        builder.Property(x => x.Configuracion).HasColumnName("configuracion");
        builder.Property(x => x.Activa).HasColumnName("activa").HasDefaultValue(true);

        builder.HasIndex(x => x.Codigo).IsUnique();
    }
}
