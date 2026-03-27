using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZuluIA_Back.Domain.Entities.Comercial;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Infrastructure.Persistence.Configurations;

public class MarcaComercialConfiguration : IEntityTypeConfiguration<MarcaComercial>
{
    public void Configure(EntityTypeBuilder<MarcaComercial> builder)
    {
        builder.ToTable("marcas_comerciales");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();
        builder.Property(x => x.Codigo).HasColumnName("codigo").HasMaxLength(50).IsRequired();
        builder.Property(x => x.Descripcion).HasColumnName("descripcion").HasMaxLength(200).IsRequired();
        builder.Property(x => x.Activo).HasColumnName("activo").IsRequired();
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");
        builder.Property(x => x.CreatedBy).HasColumnName("created_by");
        builder.Property(x => x.UpdatedBy).HasColumnName("updated_by");
        builder.HasIndex(x => x.Codigo).IsUnique();
    }
}

public class ZonaComercialConfiguration : IEntityTypeConfiguration<ZonaComercial>
{
    public void Configure(EntityTypeBuilder<ZonaComercial> builder)
    {
        builder.ToTable("zonas_comerciales");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();
        builder.Property(x => x.Codigo).HasColumnName("codigo").HasMaxLength(50).IsRequired();
        builder.Property(x => x.Descripcion).HasColumnName("descripcion").HasMaxLength(200).IsRequired();
        builder.Property(x => x.Activo).HasColumnName("activo").IsRequired();
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");
        builder.Property(x => x.CreatedBy).HasColumnName("created_by");
        builder.Property(x => x.UpdatedBy).HasColumnName("updated_by");
        builder.HasIndex(x => x.Codigo).IsUnique();
    }
}

public class JurisdiccionComercialConfiguration : IEntityTypeConfiguration<JurisdiccionComercial>
{
    public void Configure(EntityTypeBuilder<JurisdiccionComercial> builder)
    {
        builder.ToTable("jurisdicciones_comerciales");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();
        builder.Property(x => x.Codigo).HasColumnName("codigo").HasMaxLength(50).IsRequired();
        builder.Property(x => x.Descripcion).HasColumnName("descripcion").HasMaxLength(200).IsRequired();
        builder.Property(x => x.Activo).HasColumnName("activo").IsRequired();
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");
        builder.Property(x => x.CreatedBy).HasColumnName("created_by");
        builder.Property(x => x.UpdatedBy).HasColumnName("updated_by");
        builder.HasIndex(x => x.Codigo).IsUnique();
    }
}

public class MaestroAuxiliarComercialConfiguration : IEntityTypeConfiguration<MaestroAuxiliarComercial>
{
    public void Configure(EntityTypeBuilder<MaestroAuxiliarComercial> builder)
    {
        builder.ToTable("maestros_auxiliares_comerciales");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();
        builder.Property(x => x.Grupo).HasColumnName("grupo").HasMaxLength(50).IsRequired();
        builder.Property(x => x.Codigo).HasColumnName("codigo").HasMaxLength(50).IsRequired();
        builder.Property(x => x.Descripcion).HasColumnName("descripcion").HasMaxLength(200).IsRequired();
        builder.Property(x => x.Activo).HasColumnName("activo").IsRequired();
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");
        builder.Property(x => x.CreatedBy).HasColumnName("created_by");
        builder.Property(x => x.UpdatedBy).HasColumnName("updated_by");
        builder.HasIndex(x => new { x.Grupo, x.Codigo }).IsUnique();
    }
}

public class AtributoComercialConfiguration : IEntityTypeConfiguration<AtributoComercial>
{
    public void Configure(EntityTypeBuilder<AtributoComercial> builder)
    {
        builder.ToTable("atributos_comerciales");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();
        builder.Property(x => x.Codigo).HasColumnName("codigo").HasMaxLength(50).IsRequired();
        builder.Property(x => x.Descripcion).HasColumnName("descripcion").HasMaxLength(200).IsRequired();
        builder.Property(x => x.TipoDato)
            .HasColumnName("tipo_dato")
            .HasConversion(v => v.ToString().ToUpperInvariant(), v => Enum.Parse<TipoDatoAtributoComercial>(v, true))
            .HasMaxLength(20)
            .IsRequired();
        builder.Property(x => x.Activo).HasColumnName("activo").IsRequired();
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");
        builder.Property(x => x.CreatedBy).HasColumnName("created_by");
        builder.Property(x => x.UpdatedBy).HasColumnName("updated_by");
        builder.HasIndex(x => x.Codigo).IsUnique();
    }
}

public class ComprobanteItemAtributoComercialConfiguration : IEntityTypeConfiguration<ComprobanteItemAtributoComercial>
{
    public void Configure(EntityTypeBuilder<ComprobanteItemAtributoComercial> builder)
    {
        builder.ToTable("comprobantes_items_atributos");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();
        builder.Property(x => x.ComprobanteItemId).HasColumnName("comprobante_item_id").IsRequired();
        builder.Property(x => x.AtributoComercialId).HasColumnName("atributo_comercial_id").IsRequired();
        builder.Property(x => x.Valor).HasColumnName("valor").HasMaxLength(300).IsRequired();
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");
        builder.Property(x => x.CreatedBy).HasColumnName("created_by");
        builder.Property(x => x.UpdatedBy).HasColumnName("updated_by");
        builder.HasIndex(x => new { x.ComprobanteItemId, x.AtributoComercialId }).IsUnique();
    }
}
