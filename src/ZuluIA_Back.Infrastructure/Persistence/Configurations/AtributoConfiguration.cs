using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZuluIA_Back.Domain.Entities.Items;

namespace ZuluIA_Back.Infrastructure.Persistence.Configurations;

public class AtributoConfiguration : IEntityTypeConfiguration<Atributo>
{
    public void Configure(EntityTypeBuilder<Atributo> builder)
    {
        builder.ToTable("atributos");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();
        builder.Property(x => x.Descripcion).HasColumnName("descripcion").HasMaxLength(200).IsRequired();
        builder.Property(x => x.Tipo).HasColumnName("tipo").HasMaxLength(20).HasDefaultValue("texto");
        builder.Property(x => x.Requerido).HasColumnName("requerido").HasDefaultValue(false);
        builder.Property(x => x.Activo).HasColumnName("activo").HasDefaultValue(true);
    }
}

public class AtributoItemConfiguration : IEntityTypeConfiguration<AtributoItem>
{
    public void Configure(EntityTypeBuilder<AtributoItem> builder)
    {
        builder.ToTable("atributos_items");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();
        builder.Property(x => x.ItemId).HasColumnName("item_id").IsRequired();
        builder.Property(x => x.AtributoId).HasColumnName("atributo_id").IsRequired();
        builder.Property(x => x.Valor).HasColumnName("valor").HasMaxLength(500);
        builder.HasIndex(x => new { x.ItemId, x.AtributoId }).IsUnique();
    }
}
