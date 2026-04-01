using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZuluIA_Back.Domain.Entities.Contratos;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Infrastructure.Persistence.Configurations;

public class ContratoNegocioConfiguration : IEntityTypeConfiguration<Contrato>
{
    public void Configure(EntityTypeBuilder<Contrato> builder)
    {
        builder.ToTable("contratos");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();
        builder.Property(x => x.TerceroId).HasColumnName("tercero_id").IsRequired();
        builder.Property(x => x.SucursalId).HasColumnName("sucursal_id").IsRequired();
        builder.Property(x => x.MonedaId).HasColumnName("moneda_id").IsRequired();
        builder.Property(x => x.Codigo).HasColumnName("codigo").HasMaxLength(50).IsRequired();
        builder.Property(x => x.Descripcion).HasColumnName("descripcion").HasMaxLength(200).IsRequired();
        builder.Property(x => x.FechaInicio).HasColumnName("fecha_inicio").IsRequired();
        builder.Property(x => x.FechaFin).HasColumnName("fecha_fin").IsRequired();
        builder.Property(x => x.Importe).HasColumnName("importe").HasPrecision(18, 4).IsRequired();
        builder.Property(x => x.RenovacionAutomatica).HasColumnName("renovacion_automatica").IsRequired();
        builder.Property(x => x.Estado)
            .HasColumnName("estado")
            .HasConversion(v => v.ToString().ToUpperInvariant(), v => Enum.Parse<EstadoContrato>(v, true))
            .HasMaxLength(20)
            .IsRequired();
        builder.Property(x => x.Observacion).HasColumnName("observacion");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");
        builder.Property(x => x.CreatedBy).HasColumnName("created_by");
        builder.Property(x => x.UpdatedBy).HasColumnName("updated_by");
        builder.HasIndex(x => x.Codigo).IsUnique();
        builder.HasIndex(x => x.TerceroId);
        builder.HasIndex(x => x.SucursalId);
        builder.HasIndex(x => x.Estado);
    }
}

public class ContratoHistorialConfiguration : IEntityTypeConfiguration<ContratoHistorial>
{
    public void Configure(EntityTypeBuilder<ContratoHistorial> builder)
    {
        builder.ToTable("contratos_historial");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();
        builder.Property(x => x.ContratoId).HasColumnName("contrato_id").IsRequired();
        builder.Property(x => x.TipoEvento)
            .HasColumnName("tipo_evento")
            .HasConversion(v => v.ToString().ToUpperInvariant(), v => Enum.Parse<TipoEventoContrato>(v, true))
            .HasMaxLength(30)
            .IsRequired();
        builder.Property(x => x.Fecha).HasColumnName("fecha").IsRequired();
        builder.Property(x => x.Descripcion).HasColumnName("descripcion").HasMaxLength(300).IsRequired();
        builder.Property(x => x.Importe).HasColumnName("importe").HasPrecision(18, 4);
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");
        builder.Property(x => x.CreatedBy).HasColumnName("created_by");
        builder.Property(x => x.UpdatedBy).HasColumnName("updated_by");
        builder.HasIndex(x => x.ContratoId);
        builder.HasIndex(x => x.TipoEvento);
    }
}

public class ContratoImpactoConfiguration : IEntityTypeConfiguration<ContratoImpacto>
{
    public void Configure(EntityTypeBuilder<ContratoImpacto> builder)
    {
        builder.ToTable("contratos_impactos");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();
        builder.Property(x => x.ContratoId).HasColumnName("contrato_id").IsRequired();
        builder.Property(x => x.Tipo)
            .HasColumnName("tipo")
            .HasConversion(v => v.ToString().ToUpperInvariant(), v => Enum.Parse<TipoImpactoContrato>(v, true))
            .HasMaxLength(20)
            .IsRequired();
        builder.Property(x => x.Fecha).HasColumnName("fecha").IsRequired();
        builder.Property(x => x.Importe).HasColumnName("importe").HasPrecision(18, 4).IsRequired();
        builder.Property(x => x.Descripcion).HasColumnName("descripcion").HasMaxLength(300).IsRequired();
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");
        builder.Property(x => x.CreatedBy).HasColumnName("created_by");
        builder.Property(x => x.UpdatedBy).HasColumnName("updated_by");
        builder.HasIndex(x => x.ContratoId);
        builder.HasIndex(x => x.Tipo);
    }
}
