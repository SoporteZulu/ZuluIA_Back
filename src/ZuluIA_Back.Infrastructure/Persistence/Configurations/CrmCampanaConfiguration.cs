using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZuluIA_Back.Domain.Entities.CRM;

namespace ZuluIA_Back.Infrastructure.Persistence.Configurations;

public class CrmCampanaConfiguration : IEntityTypeConfiguration<CrmCampana>
{
    public void Configure(EntityTypeBuilder<CrmCampana> builder)
    {
        builder.ToTable("CRMCAMPANA");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();

        builder.Property(x => x.SucursalId).HasColumnName("sucursal_id").IsRequired();
        builder.Property(x => x.Nombre).HasColumnName("nombre").HasMaxLength(200).IsRequired();
        builder.Property(x => x.Descripcion).HasColumnName("descripcion").HasMaxLength(1000);
        builder.Property(x => x.FechaInicio).HasColumnName("fecha_inicio").IsRequired();
        builder.Property(x => x.FechaFin).HasColumnName("fecha_fin").IsRequired();
        builder.Property(x => x.Presupuesto).HasColumnName("presupuesto").HasPrecision(18, 2);
        builder.Property(x => x.Activa).HasColumnName("activa").HasDefaultValue(true);

        builder.HasIndex(x => new { x.SucursalId, x.FechaInicio });
    }
}
