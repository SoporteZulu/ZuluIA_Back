using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZuluIA_Back.Domain.Entities.Logistica;

namespace ZuluIA_Back.Infrastructure.Persistence.Configurations;

public class OrdenCargaConfiguration : IEntityTypeConfiguration<OrdenCarga>
{
    public void Configure(EntityTypeBuilder<OrdenCarga> builder)
    {
        builder.ToTable("ordenes_carga");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();
        builder.Property(x => x.CartaPorteId).HasColumnName("carta_porte_id").IsRequired();
        builder.Property(x => x.TransportistaId).HasColumnName("transportista_id");
        builder.Property(x => x.FechaCarga).HasColumnName("fecha_carga").IsRequired();
        builder.Property(x => x.Origen).HasColumnName("origen").HasMaxLength(250).IsRequired();
        builder.Property(x => x.Destino).HasColumnName("destino").HasMaxLength(250).IsRequired();
        builder.Property(x => x.Patente).HasColumnName("patente").HasMaxLength(20);
        builder.Property(x => x.Confirmada).HasColumnName("confirmada").IsRequired();
        builder.Property(x => x.Observacion).HasColumnName("observacion").HasMaxLength(500);
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");
        builder.Property(x => x.CreatedBy).HasColumnName("created_by");
        builder.Property(x => x.UpdatedBy).HasColumnName("updated_by");
        builder.HasIndex(x => x.CartaPorteId);
        builder.HasIndex(x => x.TransportistaId);
        builder.HasIndex(x => x.FechaCarga);
    }
}
