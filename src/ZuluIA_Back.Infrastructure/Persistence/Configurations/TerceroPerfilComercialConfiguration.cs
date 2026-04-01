using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZuluIA_Back.Domain.Entities.Terceros;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Infrastructure.Persistence.Configurations;

public class TerceroPerfilComercialConfiguration : IEntityTypeConfiguration<TerceroPerfilComercial>
{
    public void Configure(EntityTypeBuilder<TerceroPerfilComercial> builder)
    {
        builder.ToTable("terceros_perfiles_comerciales");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .UseIdentityColumn();

        builder.Property(x => x.TerceroId)
            .HasColumnName("tercero_id")
            .IsRequired();

        builder.Property(x => x.ZonaComercialId)
            .HasColumnName("zona_comercial_id");

        builder.Property(x => x.Rubro)
            .HasColumnName("rubro")
            .HasMaxLength(100);

        builder.Property(x => x.Subrubro)
            .HasColumnName("subrubro")
            .HasMaxLength(100);

        builder.Property(x => x.Sector)
            .HasColumnName("sector")
            .HasMaxLength(100);

        builder.Property(x => x.CondicionCobranza)
            .HasColumnName("condicion_cobranza")
            .HasMaxLength(150);

        builder.Property(x => x.RiesgoCrediticio)
            .HasColumnName("riesgo_crediticio")
            .HasConversion(v => v.ToString().ToUpperInvariant(), v => Enum.Parse<RiesgoCrediticioComercial>(v, true))
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.SaldoMaximoVigente)
            .HasColumnName("saldo_maximo_vigente")
            .HasPrecision(18, 4);

        builder.Property(x => x.VigenciaSaldo)
            .HasColumnName("vigencia_saldo")
            .HasMaxLength(100);

        builder.Property(x => x.VigenciaSaldoDesde)
            .HasColumnName("vigencia_saldo_desde")
            .HasColumnType("date");

        builder.Property(x => x.VigenciaSaldoHasta)
            .HasColumnName("vigencia_saldo_hasta")
            .HasColumnType("date");

        builder.Property(x => x.CondicionVenta)
            .HasColumnName("condicion_venta")
            .HasMaxLength(150);

        builder.Property(x => x.PlazoCobro)
            .HasColumnName("plazo_cobro")
            .HasMaxLength(150);

        builder.Property(x => x.FacturadorPorDefecto)
            .HasColumnName("facturador_por_defecto")
            .HasMaxLength(150);

        builder.Property(x => x.MinimoFacturaMipymes)
            .HasColumnName("minimo_factura_mipymes")
            .HasPrecision(18, 4);

        builder.Property(x => x.ObservacionComercial)
            .HasColumnName("observacion_comercial")
            .HasMaxLength(1000);

        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");
        builder.Property(x => x.CreatedBy).HasColumnName("created_by");
        builder.Property(x => x.UpdatedBy).HasColumnName("updated_by");

        builder.HasIndex(x => x.TerceroId).IsUnique();
        builder.HasIndex(x => x.ZonaComercialId);

        builder.HasOne<Tercero>()
            .WithMany()
            .HasForeignKey(x => x.TerceroId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<ZuluIA_Back.Domain.Entities.Comercial.ZonaComercial>()
            .WithMany()
            .HasForeignKey(x => x.ZonaComercialId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
