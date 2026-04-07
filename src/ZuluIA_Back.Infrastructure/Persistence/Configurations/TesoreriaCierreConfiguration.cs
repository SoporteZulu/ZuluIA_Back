using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZuluIA_Back.Domain.Entities.Finanzas;

namespace ZuluIA_Back.Infrastructure.Persistence.Configurations;

public class TesoreriaCierreConfiguration : IEntityTypeConfiguration<TesoreriaCierre>
{
    public void Configure(EntityTypeBuilder<TesoreriaCierre> builder)
    {
        builder.ToTable("tesoreria_cierres");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();
        builder.Property(x => x.SucursalId).HasColumnName("sucursal_id").IsRequired();
        builder.Property(x => x.CajaCuentaId).HasColumnName("caja_cuenta_id").IsRequired();
        builder.Property(x => x.NroCierre).HasColumnName("nro_cierre").IsRequired();
        builder.Property(x => x.Fecha).HasColumnName("fecha").IsRequired();
        builder.Property(x => x.EsApertura).HasColumnName("es_apertura").IsRequired();
        builder.Property(x => x.SaldoInformado).HasColumnName("saldo_informado").HasPrecision(18, 4).IsRequired();
        builder.Property(x => x.SaldoSistema).HasColumnName("saldo_sistema").HasPrecision(18, 4).IsRequired();
        builder.Property(x => x.TotalIngresos).HasColumnName("total_ingresos").HasPrecision(18, 4).IsRequired();
        builder.Property(x => x.TotalEgresos).HasColumnName("total_egresos").HasPrecision(18, 4).IsRequired();
        builder.Property(x => x.CantidadMovimientos).HasColumnName("cantidad_movimientos").IsRequired();
        builder.Property(x => x.Observacion).HasColumnName("observacion").HasMaxLength(500);
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");
        builder.Property(x => x.CreatedBy).HasColumnName("created_by");
        builder.Property(x => x.UpdatedBy).HasColumnName("updated_by");
        builder.HasIndex(x => new { x.CajaCuentaId, x.NroCierre, x.EsApertura });
        builder.HasIndex(x => x.Fecha);
    }
}
