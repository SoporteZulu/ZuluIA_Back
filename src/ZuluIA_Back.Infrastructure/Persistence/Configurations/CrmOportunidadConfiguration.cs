using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZuluIA_Back.Domain.Entities.CRM;

namespace ZuluIA_Back.Infrastructure.Persistence.Configurations;

public class CrmOportunidadConfiguration : IEntityTypeConfiguration<CrmOportunidad>
{
    public void Configure(EntityTypeBuilder<CrmOportunidad> builder)
    {
        builder.ToTable("CRMOPORTUNIDADES");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();

        builder.Property(x => x.ClienteId).HasColumnName("cliente_id").IsRequired();
        builder.Property(x => x.ContactoPrincipalId).HasColumnName("contacto_principal_id");
        builder.Property(x => x.Titulo).HasColumnName("titulo").HasMaxLength(200).IsRequired();
        builder.Property(x => x.Etapa).HasColumnName("etapa").HasMaxLength(40).IsRequired();
        builder.Property(x => x.Probabilidad).HasColumnName("probabilidad").IsRequired();
        builder.Property(x => x.MontoEstimado).HasColumnName("monto_estimado").HasPrecision(18, 2);
        builder.Property(x => x.Moneda).HasColumnName("moneda").HasMaxLength(10).IsRequired();
        builder.Property(x => x.FechaApertura).HasColumnName("fecha_apertura").IsRequired();
        builder.Property(x => x.FechaEstimadaCierre).HasColumnName("fecha_estimada_cierre");
        builder.Property(x => x.ResponsableId).HasColumnName("responsable_id");
        builder.Property(x => x.Origen).HasColumnName("origen").HasMaxLength(30).IsRequired();
        builder.Property(x => x.MotivoPerdida).HasColumnName("motivo_perdida").HasMaxLength(500);
        builder.Property(x => x.Notas).HasColumnName("notas").HasMaxLength(2000);
        builder.Property(x => x.Activa).HasColumnName("activa").HasDefaultValue(true);
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");
        builder.Property(x => x.CreatedBy).HasColumnName("created_by");
        builder.Property(x => x.UpdatedBy).HasColumnName("updated_by");

        builder.HasIndex(x => new { x.ClienteId, x.Activa });
        builder.HasIndex(x => new { x.ResponsableId, x.Etapa });
    }
}
