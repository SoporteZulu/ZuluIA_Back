using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZuluIA_Back.Domain.Entities.Extras;

namespace ZuluIA_Back.Infrastructure.Persistence.Configurations;

public class MatriculaConfiguration : IEntityTypeConfiguration<Matricula>
{
    public void Configure(EntityTypeBuilder<Matricula> builder)
    {
        builder.ToTable("MATRICULAS");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();

        builder.Property(x => x.TerceroId).HasColumnName("tercero_id").IsRequired();
        builder.Property(x => x.SucursalId).HasColumnName("sucursal_id").IsRequired();
        builder.Property(x => x.NroMatricula).HasColumnName("nro_matricula").HasMaxLength(60).IsRequired();
        builder.Property(x => x.Descripcion).HasColumnName("descripcion").HasMaxLength(500);
        builder.Property(x => x.FechaAlta).HasColumnName("fecha_alta").IsRequired();
        builder.Property(x => x.FechaVencimiento).HasColumnName("fecha_vencimiento");
        builder.Property(x => x.Activa).HasColumnName("activa").HasDefaultValue(true);

        builder.HasIndex(x => new { x.SucursalId, x.NroMatricula }).IsUnique();
        builder.HasIndex(x => x.TerceroId);
    }
}
