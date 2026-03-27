using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZuluIA_Back.Domain.Entities.Documentos;

namespace ZuluIA_Back.Infrastructure.Persistence.Configurations;

public class MesaEntradaConfiguration : IEntityTypeConfiguration<MesaEntrada>
{
    public void Configure(EntityTypeBuilder<MesaEntrada> builder)
    {
        builder.ToTable("MESAENTRADA");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();

        builder.Property(x => x.SucursalId).HasColumnName("sucursal_id").IsRequired();
        builder.Property(x => x.TipoId).HasColumnName("tipo_id").IsRequired();
        builder.Property(x => x.TerceroId).HasColumnName("tercero_id");
        builder.Property(x => x.EstadoId).HasColumnName("estado_id");
        builder.Property(x => x.NroDocumento).HasColumnName("nro_documento").HasMaxLength(60).IsRequired();
        builder.Property(x => x.Asunto).HasColumnName("asunto").HasMaxLength(300).IsRequired();
        builder.Property(x => x.FechaIngreso).HasColumnName("fecha_ingreso").IsRequired();
        builder.Property(x => x.FechaVencimiento).HasColumnName("fecha_vencimiento");
        builder.Property(x => x.Observacion).HasColumnName("observacion").HasMaxLength(1000);
        builder.Property(x => x.AsignadoA).HasColumnName("asignado_a");
        builder.Property(x => x.EstadoFlow).HasColumnName("estado_flow").IsRequired();

        builder.HasIndex(x => new { x.SucursalId, x.FechaIngreso });
        builder.HasIndex(x => x.NroDocumento);
    }
}
