using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZuluIA_Back.Domain.Entities.Stock;

namespace ZuluIA_Back.Infrastructure.Persistence.Configurations;

public class InventarioConteoConfiguration : IEntityTypeConfiguration<InventarioConteo>
{
    public void Configure(EntityTypeBuilder<InventarioConteo> builder)
    {
        builder.ToTable("inventario_conteo");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();

        builder.Property(x => x.UsuarioId)
               .HasColumnName("usuario_id").IsRequired();

        builder.Property(x => x.FechaApertura)
               .HasColumnName("fecha_apertura").IsRequired();

        builder.Property(x => x.FechaCierre)
               .HasColumnName("fecha_cierre");

        builder.Property(x => x.FechaAlta)
               .HasColumnName("fecha_alta").IsRequired();

        builder.Property(x => x.NroAuditoria)
               .HasColumnName("nro_auditoria").IsRequired();

        builder.HasIndex(x => x.NroAuditoria).IsUnique();
    }
}
