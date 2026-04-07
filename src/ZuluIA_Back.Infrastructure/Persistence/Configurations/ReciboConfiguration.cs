using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZuluIA_Back.Domain.Entities.Finanzas;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Infrastructure.Persistence.Configurations;

public class ReciboConfiguration : IEntityTypeConfiguration<Recibo>
{
    public void Configure(EntityTypeBuilder<Recibo> builder)
    {
        builder.ToTable("recibos");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();

        builder.Property(x => x.SucursalId).HasColumnName("sucursal_id").IsRequired();
        builder.Property(x => x.TerceroId).HasColumnName("tercero_id").IsRequired();
        builder.Property(x => x.Fecha).HasColumnName("fecha").IsRequired();
        builder.Property(x => x.Serie).HasColumnName("serie").HasMaxLength(10).IsRequired();
        builder.Property(x => x.Numero).HasColumnName("numero").IsRequired();
        builder.Property(x => x.Total).HasColumnName("total").HasPrecision(18, 4);
        builder.Property(x => x.Observacion).HasColumnName("observacion").HasMaxLength(500);
        builder.Property(x => x.CobroId).HasColumnName("cobro_id");

        builder.Property(x => x.Estado)
               .HasColumnName("estado")
               .HasConversion(
                   v => v.ToString().ToUpperInvariant(),
                   v => Enum.Parse<EstadoRecibo>(v, true))
               .HasMaxLength(20)
               .IsRequired();

        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");
        builder.Property(x => x.CreatedBy).HasColumnName("created_by");
        builder.Property(x => x.UpdatedBy).HasColumnName("updated_by");

        builder.HasMany(x => x.Items)
               .WithOne()
               .HasForeignKey(x => x.ReciboId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.SucursalId);
        builder.HasIndex(x => x.TerceroId);
        builder.HasIndex(x => new { x.SucursalId, x.Serie, x.Numero }).IsUnique();
    }
}

public class ReciboItemConfiguration : IEntityTypeConfiguration<ReciboItem>
{
    public void Configure(EntityTypeBuilder<ReciboItem> builder)
    {
        builder.ToTable("recibos_items");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();
        builder.Property(x => x.ReciboId).HasColumnName("recibo_id").IsRequired();
        builder.Property(x => x.Descripcion).HasColumnName("descripcion").HasMaxLength(300).IsRequired();
        builder.Property(x => x.Importe).HasColumnName("importe").HasPrecision(18, 4).IsRequired();
        builder.HasIndex(x => x.ReciboId);
    }
}
