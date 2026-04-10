using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZuluIA_Back.Domain.Entities.CRM;

namespace ZuluIA_Back.Infrastructure.Persistence.Configurations;

public class CrmClienteConfiguration : IEntityTypeConfiguration<CrmCliente>
{
    public void Configure(EntityTypeBuilder<CrmCliente> builder)
    {
        builder.ToTable("CRMCLIENTES");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();

        builder.Property(x => x.TerceroId).HasColumnName("tercero_id").IsRequired();
        builder.Property(x => x.TipoCliente).HasColumnName("tipo_cliente").HasMaxLength(30).IsRequired();
        builder.Property(x => x.Segmento).HasColumnName("segmento").HasMaxLength(30).IsRequired();
        builder.Property(x => x.Industria).HasColumnName("industria").HasMaxLength(120);
        builder.Property(x => x.Pais).HasColumnName("pais").HasMaxLength(120).IsRequired();
        builder.Property(x => x.Provincia).HasColumnName("provincia").HasMaxLength(120);
        builder.Property(x => x.Ciudad).HasColumnName("ciudad").HasMaxLength(120);
        builder.Property(x => x.Direccion).HasColumnName("direccion").HasMaxLength(300);
        builder.Property(x => x.OrigenCliente).HasColumnName("origen_cliente").HasMaxLength(30).IsRequired();
        builder.Property(x => x.EstadoRelacion).HasColumnName("estado_relacion").HasMaxLength(30).IsRequired();
        builder.Property(x => x.ResponsableId).HasColumnName("responsable_id");
        builder.Property(x => x.NotasGenerales).HasColumnName("notas_generales").HasMaxLength(2000);
        builder.Property(x => x.Activo).HasColumnName("activo").HasDefaultValue(true);
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");
        builder.Property(x => x.CreatedBy).HasColumnName("created_by");
        builder.Property(x => x.UpdatedBy).HasColumnName("updated_by");

        builder.HasIndex(x => x.TerceroId).IsUnique();
        builder.HasIndex(x => new { x.ResponsableId, x.EstadoRelacion });
    }
}
