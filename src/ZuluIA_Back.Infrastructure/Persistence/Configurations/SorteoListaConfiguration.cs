using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZuluIA_Back.Domain.Entities.Extras;

namespace ZuluIA_Back.Infrastructure.Persistence.Configurations;

public class SorteoListaConfiguration : IEntityTypeConfiguration<SorteoLista>
{
    public void Configure(EntityTypeBuilder<SorteoLista> builder)
    {
        builder.ToTable("SORTEOLISTA");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();

        builder.Property(x => x.SucursalId).HasColumnName("sucursal_id").IsRequired();
        builder.Property(x => x.TipoId).HasColumnName("tipo_id").IsRequired();
        builder.Property(x => x.Descripcion).HasColumnName("descripcion").HasMaxLength(300).IsRequired();
        builder.Property(x => x.FechaInicio).HasColumnName("fecha_inicio").IsRequired();
        builder.Property(x => x.FechaFin).HasColumnName("fecha_fin").IsRequired();
        builder.Property(x => x.Activa).HasColumnName("activa").HasDefaultValue(true);

        builder.HasIndex(x => new { x.SucursalId, x.TipoId, x.FechaInicio });
    }
}
