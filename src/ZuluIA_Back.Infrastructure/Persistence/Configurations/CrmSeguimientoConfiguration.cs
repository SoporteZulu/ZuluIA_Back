using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZuluIA_Back.Domain.Entities.CRM;

namespace ZuluIA_Back.Infrastructure.Persistence.Configurations;

public class CrmSeguimientoConfiguration : IEntityTypeConfiguration<CrmSeguimiento>
{
    public void Configure(EntityTypeBuilder<CrmSeguimiento> builder)
    {
        builder.ToTable("CRMSEGUIMIENTOS");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();

        builder.Property(x => x.SucursalId).HasColumnName("sucursal_id").IsRequired();
        builder.Property(x => x.TerceroId).HasColumnName("tercero_id").IsRequired();
        builder.Property(x => x.MotivoId).HasColumnName("motivo_id");
        builder.Property(x => x.InteresId).HasColumnName("interes_id");
        builder.Property(x => x.CampanaId).HasColumnName("campana_id");
        builder.Property(x => x.Fecha).HasColumnName("fecha").IsRequired();
        builder.Property(x => x.Descripcion).HasColumnName("descripcion").HasMaxLength(1000).IsRequired();
        builder.Property(x => x.ProximaAccion).HasColumnName("proxima_accion");
        builder.Property(x => x.UsuarioId).HasColumnName("usuario_id");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");
        builder.Property(x => x.CreatedBy).HasColumnName("created_by");
        builder.Property(x => x.UpdatedBy).HasColumnName("updated_by");

        builder.HasIndex(x => new { x.SucursalId, x.Fecha });
        builder.HasIndex(x => x.TerceroId);
        builder.HasIndex(x => x.CampanaId);
    }
}
