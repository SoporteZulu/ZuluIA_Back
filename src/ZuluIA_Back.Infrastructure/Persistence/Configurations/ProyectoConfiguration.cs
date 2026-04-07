using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZuluIA_Back.Domain.Entities.Proyectos;

namespace ZuluIA_Back.Infrastructure.Persistence.Configurations;

public class ProyectoConfiguration : IEntityTypeConfiguration<Proyecto>
{
    public void Configure(EntityTypeBuilder<Proyecto> builder)
    {
        builder.ToTable("proyecto");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();
        builder.Property(x => x.Codigo).HasColumnName("codigo").HasMaxLength(30).IsRequired();
        builder.Property(x => x.Descripcion).HasColumnName("descripcion").HasMaxLength(200).IsRequired();
        builder.Property(x => x.Observacion).HasColumnName("observacion").HasMaxLength(1000);
        builder.Property(x => x.Estado).HasColumnName("estado").HasMaxLength(20).HasDefaultValue("activo");
        builder.Property(x => x.FechaInicio).HasColumnName("fecha_inicio");
        builder.Property(x => x.FechaFin).HasColumnName("fecha_fin");
        builder.Property(x => x.SucursalId).HasColumnName("sucursal_id").IsRequired();
        builder.Property(x => x.TerceroId).HasColumnName("tercero_id");
        builder.Property(x => x.Anulada).HasColumnName("anulada").HasDefaultValue(false);
        builder.Property(x => x.TotalCuotas).HasColumnName("total_cuotas").HasDefaultValue(0);
        builder.Property(x => x.SoloPadre).HasColumnName("solo_padre").HasDefaultValue(false);
        builder.HasIndex(x => new { x.SucursalId, x.Codigo }).IsUnique();
    }
}

public class ComprobanteProyectoConfiguration : IEntityTypeConfiguration<ComprobanteProyecto>
{
    public void Configure(EntityTypeBuilder<ComprobanteProyecto> builder)
    {
        builder.ToTable("comprobante_proyecto");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();
        builder.Property(x => x.ComprobanteId).HasColumnName("comprobante_id").IsRequired();
        builder.Property(x => x.ProyectoId).HasColumnName("proyecto_id").IsRequired();
        builder.Property(x => x.Porcentaje).HasColumnName("porcentaje").HasColumnType("numeric(7,4)");
        builder.Property(x => x.Observacion).HasColumnName("observacion").HasMaxLength(500);
        builder.Property(x => x.NroCuota).HasColumnName("nro_cuota").HasDefaultValue(0);
        builder.Property(x => x.Importe).HasColumnName("importe").HasColumnType("numeric(18,4)");
        builder.Property(x => x.Deshabilitada).HasColumnName("deshabilitada").HasDefaultValue(false);
        builder.HasIndex(x => new { x.ComprobanteId, x.ProyectoId });
    }
}
