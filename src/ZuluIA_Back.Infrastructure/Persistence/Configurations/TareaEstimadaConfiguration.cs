using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZuluIA_Back.Domain.Entities.Proyectos;

namespace ZuluIA_Back.Infrastructure.Persistence.Configurations;

public class TareaEstimadaConfiguration : IEntityTypeConfiguration<TareaEstimada>
{
    public void Configure(EntityTypeBuilder<TareaEstimada> builder)
    {
        builder.ToTable("XERPTAREASESTIMADAS");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();

        builder.Property(x => x.ProyectoId).HasColumnName("proyecto_id").IsRequired();
        builder.Property(x => x.SucursalId).HasColumnName("sucursal_id").IsRequired();
        builder.Property(x => x.AsignadoA).HasColumnName("asignado_a");
        builder.Property(x => x.Descripcion).HasColumnName("descripcion").HasMaxLength(500).IsRequired();
        builder.Property(x => x.FechaDesde).HasColumnName("fecha_desde").IsRequired();
        builder.Property(x => x.FechaHasta).HasColumnName("fecha_hasta").IsRequired();
        builder.Property(x => x.HorasEstimadas).HasColumnName("horas_estimadas").HasPrecision(12, 2).IsRequired();
        builder.Property(x => x.Observacion).HasColumnName("observacion").HasMaxLength(1000);
        builder.Property(x => x.Activa).HasColumnName("activa").HasDefaultValue(true);

        builder.HasIndex(x => new { x.ProyectoId, x.SucursalId, x.FechaDesde });
    }
}
