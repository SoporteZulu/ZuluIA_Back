using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZuluIA_Back.Domain.Entities.Items;

namespace ZuluIA_Back.Infrastructure.Persistence.Configurations;

public class DepositoConfiguration : IEntityTypeConfiguration<Deposito>
{
    public void Configure(EntityTypeBuilder<Deposito> builder)
    {
        builder.ToTable("depositos");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();

        builder.Property(x => x.SucursalId)
               .HasColumnName("sucursal_id")
               .IsRequired();

        builder.Property(x => x.Descripcion)
               .HasColumnName("descripcion")
               .HasMaxLength(200)
               .IsRequired();

        builder.Property(x => x.EsDefault)
               .HasColumnName("es_default")
               .HasDefaultValue(false);

        builder.Property(x => x.Activo)
               .HasColumnName("activo")
               .HasDefaultValue(true);

        builder.Property(x => x.CreatedAt)
               .HasColumnName("created_at");

        builder.HasIndex(x => x.SucursalId);
        builder.HasIndex(x => new { x.SucursalId, x.EsDefault });
    }
}