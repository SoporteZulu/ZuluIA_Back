using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZuluIA_Back.Domain.Entities.Proyectos;

namespace ZuluIA_Back.Infrastructure.Persistence.Configurations;

public class TareaRealConfiguration : IEntityTypeConfiguration<TareaReal>
{
    public void Configure(EntityTypeBuilder<TareaReal> builder)
    {
        builder.ToTable("XERPTAREASREALES");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();

        builder.Property(x => x.ProyectoId).HasColumnName("proyecto_id").IsRequired();
        builder.Property(x => x.SucursalId).HasColumnName("sucursal_id").IsRequired();
        builder.Property(x => x.TareaEstimadaId).HasColumnName("tarea_estimada_id");
        builder.Property(x => x.UsuarioId).HasColumnName("usuario_id").IsRequired();
        builder.Property(x => x.Fecha).HasColumnName("fecha").IsRequired();
        builder.Property(x => x.Descripcion).HasColumnName("descripcion").HasMaxLength(500).IsRequired();
        builder.Property(x => x.HorasReales).HasColumnName("horas_reales").HasPrecision(12, 2).IsRequired();
        builder.Property(x => x.Aprobada).HasColumnName("aprobada").HasDefaultValue(false);
        builder.Property(x => x.Observacion).HasColumnName("observacion").HasMaxLength(1000);

        builder.HasIndex(x => new { x.ProyectoId, x.SucursalId, x.Fecha });
    }
}
