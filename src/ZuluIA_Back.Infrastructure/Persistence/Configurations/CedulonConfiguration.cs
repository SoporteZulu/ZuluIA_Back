using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZuluIA_Back.Domain.Entities.Finanzas;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Infrastructure.Persistence.Configurations;

public class CedulonConfiguration : IEntityTypeConfiguration<Cedulon>
{
    public void Configure(EntityTypeBuilder<Cedulon> builder)
    {
        builder.ToTable("cedulones");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();

        builder.Property(x => x.TerceroId)
               .HasColumnName("tercero_id").IsRequired();

        builder.Property(x => x.SucursalId)
               .HasColumnName("sucursal_id").IsRequired();

        builder.Property(x => x.PlanPagoId)
               .HasColumnName("plan_pago_id");

        builder.Property(x => x.PlanGeneralColegioId)
               .HasColumnName("plan_general_colegio_id");

        builder.Property(x => x.LoteColegioId)
               .HasColumnName("lote_colegio_id");

        builder.Property(x => x.ComprobanteId)
               .HasColumnName("comprobante_id");

        builder.Property(x => x.NroCedulon)
               .HasColumnName("nro_cedulon")
               .HasMaxLength(50).IsRequired();

        builder.Property(x => x.FechaEmision)
               .HasColumnName("fecha_emision").IsRequired();

        builder.Property(x => x.FechaVencimiento)
               .HasColumnName("fecha_vencimiento").IsRequired();

        builder.Property(x => x.Importe)
               .HasColumnName("importe")
               .HasPrecision(18, 4).IsRequired();

        builder.Property(x => x.ImportePagado)
               .HasColumnName("importe_pagado")
               .HasPrecision(18, 4)
               .HasDefaultValue(0m);

        builder.Property(x => x.Estado)
               .HasColumnName("estado")
               .HasConversion(
                   v => v.ToString().ToUpperInvariant(),
                   v => Enum.Parse<EstadoCedulon>(v, true))
               .HasMaxLength(20).IsRequired();

        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");
        builder.Property(x => x.CreatedBy).HasColumnName("created_by");
        builder.Property(x => x.UpdatedBy).HasColumnName("updated_by");

        builder.HasIndex(x => x.NroCedulon).IsUnique();
        builder.HasIndex(x => x.TerceroId);
        builder.HasIndex(x => x.Estado);
        builder.HasIndex(x => x.FechaVencimiento);
        builder.HasIndex(x => x.LoteColegioId);
        builder.HasIndex(x => x.PlanGeneralColegioId);
    }
}