using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZuluIA_Back.Domain.Entities.Finanzas;

namespace ZuluIA_Back.Infrastructure.Persistence.Configurations;

public class RankingFinancieroConfiguration : IEntityTypeConfiguration<RankingFinanciero>
{
    public void Configure(EntityTypeBuilder<RankingFinanciero> builder)
    {
        builder.ToTable("rankings_financiero");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();
        builder.Property(x => x.SucursalId).HasColumnName("sucursal_id").IsRequired();
        builder.Property(x => x.Periodo).HasColumnName("periodo").IsRequired();
        builder.Property(x => x.TerceroId).HasColumnName("tercero_id").IsRequired();
        builder.Property(x => x.SaldoPromedio).HasColumnName("saldo_promedio").HasPrecision(18, 2);
        builder.Property(x => x.DiasPromPago).HasColumnName("dias_prom_pago").HasPrecision(10, 2);
        builder.Property(x => x.LimiteCredito).HasColumnName("limite_credito").HasPrecision(18, 2);
        builder.Property(x => x.Posicion).HasColumnName("posicion");
        builder.Property(x => x.Categoria).HasColumnName("categoria").HasMaxLength(5);
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");
        builder.Property(x => x.CreatedBy).HasColumnName("created_by");
        builder.Property(x => x.UpdatedBy).HasColumnName("updated_by");
        builder.HasIndex(x => new { x.SucursalId, x.Periodo, x.TerceroId }).IsUnique();
    }
}
