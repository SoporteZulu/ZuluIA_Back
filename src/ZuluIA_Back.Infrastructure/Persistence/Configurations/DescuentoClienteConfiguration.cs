using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZuluIA_Back.Domain.Entities.Terceros;

namespace ZuluIA_Back.Infrastructure.Persistence.Configurations;

public class DescuentoClienteConfiguration : IEntityTypeConfiguration<DescuentoCliente>
{
    public void Configure(EntityTypeBuilder<DescuentoCliente> builder)
    {
        builder.ToTable("descuentos_clientes");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();
        builder.Property(x => x.TerceroId).HasColumnName("tercero_id").IsRequired();
        builder.Property(x => x.SucursalId).HasColumnName("sucursal_id").IsRequired();
        builder.Property(x => x.ItemId).HasColumnName("item_id");
        builder.Property(x => x.CategoriaItemId).HasColumnName("categoria_item_id");
        builder.Property(x => x.Porcentaje).HasColumnName("porcentaje").HasPrecision(8, 4).IsRequired();
        builder.Property(x => x.VigenciaDesde).HasColumnName("vigencia_desde");
        builder.Property(x => x.VigenciaHasta).HasColumnName("vigencia_hasta");
        builder.Property(x => x.Activo).HasColumnName("activo").HasDefaultValue(true);
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");
        builder.Property(x => x.CreatedBy).HasColumnName("created_by");
        builder.Property(x => x.UpdatedBy).HasColumnName("updated_by");
        builder.HasIndex(x => x.TerceroId);
        builder.HasIndex(x => x.SucursalId);
        builder.HasIndex(x => x.ItemId);
    }
}
