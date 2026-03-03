using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZuluIA_Back.Domain.Entities.Configuracion;

namespace ZuluIA_Back.Infrastructure.Persistence.Configurations;

public class ConfiguracionSistemaConfiguration : IEntityTypeConfiguration<ConfiguracionSistema>
{
    public void Configure(EntityTypeBuilder<ConfiguracionSistema> builder)
    {
        builder.ToTable("config");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();

        builder.Property(x => x.Campo)
            .HasColumnName("campo")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.Valor).HasColumnName("valor");
        builder.Property(x => x.TipoDato).HasColumnName("tipo_dato").HasDefaultValue((short)1);
        builder.Property(x => x.Descripcion).HasColumnName("descripcion").HasMaxLength(300);
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");

        builder.HasIndex(x => x.Campo).IsUnique();
    }
}