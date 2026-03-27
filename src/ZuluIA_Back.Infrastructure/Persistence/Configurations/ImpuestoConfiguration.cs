using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZuluIA_Back.Domain.Entities.Impuestos;

namespace ZuluIA_Back.Infrastructure.Persistence.Configurations;

public class ImpuestoConfiguration : IEntityTypeConfiguration<Impuesto>
{
    public void Configure(EntityTypeBuilder<Impuesto> builder)
    {
        builder.ToTable("impuesto");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();
        builder.Property(x => x.Codigo).HasColumnName("codigo").HasMaxLength(20).IsRequired();
        builder.Property(x => x.Descripcion).HasColumnName("descripcion").HasMaxLength(200).IsRequired();
        builder.Property(x => x.Observacion).HasColumnName("observacion").HasMaxLength(500);
        builder.Property(x => x.Alicuota).HasColumnName("alicuota").HasColumnType("numeric(10,4)");
        builder.Property(x => x.MinimoBaseCalculo).HasColumnName("minimo_base_calculo").HasColumnType("numeric(18,4)");
        builder.Property(x => x.Tipo).HasColumnName("tipo").HasMaxLength(30).IsRequired();
        builder.Property(x => x.Activo).HasColumnName("activo").HasDefaultValue(true);
        builder.HasIndex(x => x.Codigo).IsUnique();
    }
}

public class ImpuestoPorPersonaConfiguration : IEntityTypeConfiguration<ImpuestoPorPersona>
{
    public void Configure(EntityTypeBuilder<ImpuestoPorPersona> builder)
    {
        builder.ToTable("impuesto_por_persona");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();
        builder.Property(x => x.ImpuestoId).HasColumnName("impuesto_id").IsRequired();
        builder.Property(x => x.TerceroId).HasColumnName("tercero_id").IsRequired();
        builder.Property(x => x.Descripcion).HasColumnName("descripcion").HasMaxLength(200);
        builder.Property(x => x.Observacion).HasColumnName("observacion").HasMaxLength(500);
        builder.HasIndex(x => new { x.ImpuestoId, x.TerceroId }).IsUnique();
    }
}

public class ImpuestoPorItemConfiguration : IEntityTypeConfiguration<ImpuestoPorItem>
{
    public void Configure(EntityTypeBuilder<ImpuestoPorItem> builder)
    {
        builder.ToTable("impuesto_por_item");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();
        builder.Property(x => x.ImpuestoId).HasColumnName("impuesto_id").IsRequired();
        builder.Property(x => x.ItemId).HasColumnName("item_id").IsRequired();
        builder.Property(x => x.Descripcion).HasColumnName("descripcion").HasMaxLength(200);
        builder.Property(x => x.Observacion).HasColumnName("observacion").HasMaxLength(500);
        builder.HasIndex(x => new { x.ImpuestoId, x.ItemId }).IsUnique();
    }
}

public class ImpuestoPorSucursalConfiguration : IEntityTypeConfiguration<ImpuestoPorSucursal>
{
    public void Configure(EntityTypeBuilder<ImpuestoPorSucursal> builder)
    {
        builder.ToTable("impuesto_por_sucursal");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();
        builder.Property(x => x.ImpuestoId).HasColumnName("impuesto_id").IsRequired();
        builder.Property(x => x.SucursalId).HasColumnName("sucursal_id").IsRequired();
        builder.Property(x => x.Descripcion).HasColumnName("descripcion").HasMaxLength(200);
        builder.Property(x => x.Observacion).HasColumnName("observacion").HasMaxLength(500);
        builder.HasIndex(x => new { x.ImpuestoId, x.SucursalId }).IsUnique();
    }
}
