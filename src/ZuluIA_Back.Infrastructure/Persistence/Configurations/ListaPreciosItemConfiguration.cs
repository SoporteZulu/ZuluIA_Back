using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZuluIA_Back.Domain.Entities.Precios;

namespace ZuluIA_Back.Infrastructure.Persistence.Configurations;

public class ListaPreciosItemConfiguration : IEntityTypeConfiguration<ListaPreciosItem>
{
    public void Configure(EntityTypeBuilder<ListaPreciosItem> builder)
    {
        builder.ToTable("lista_precios_items");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();

        builder.Property(x => x.ListaId)
               .HasColumnName("lista_id")
               .IsRequired();

        builder.Property(x => x.ItemId)
               .HasColumnName("item_id")
               .IsRequired();

        builder.Property(x => x.Precio)
               .HasColumnName("precio")
               .HasPrecision(18, 4)
               .IsRequired();

        builder.Property(x => x.DescuentoPct)
               .HasColumnName("descuento_pct")
               .HasPrecision(5, 2)
               .HasDefaultValue(0m);

        builder.Property(x => x.UpdatedAt)
               .HasColumnName("updated_at");

        // Ignorar propiedad calculada — no va a DB
        builder.Ignore(x => x.PrecioFinal);

        builder.HasIndex(x => new { x.ListaId, x.ItemId }).IsUnique();
        builder.HasIndex(x => x.ItemId);
    }
}