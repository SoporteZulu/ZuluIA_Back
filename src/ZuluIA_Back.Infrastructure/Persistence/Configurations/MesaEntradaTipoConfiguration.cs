using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZuluIA_Back.Domain.Entities.Documentos;

namespace ZuluIA_Back.Infrastructure.Persistence.Configurations;

public class MesaEntradaTipoConfiguration : IEntityTypeConfiguration<MesaEntradaTipo>
{
    public void Configure(EntityTypeBuilder<MesaEntradaTipo> builder)
    {
        builder.ToTable("MESAENTRADATIPOS");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();

        builder.Property(x => x.Codigo).HasColumnName("codigo").HasMaxLength(30).IsRequired();
        builder.Property(x => x.Descripcion).HasColumnName("descripcion").HasMaxLength(200).IsRequired();
        builder.Property(x => x.Activo).HasColumnName("activo").HasDefaultValue(true);

        builder.HasIndex(x => x.Codigo).IsUnique();
    }
}
