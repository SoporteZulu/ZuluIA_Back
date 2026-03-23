using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZuluIA_Back.Domain.Entities.Finanzas;

namespace ZuluIA_Back.Infrastructure.Persistence.Configurations;

public class ChequeraConfiguration : IEntityTypeConfiguration<Chequera>
{
    public void Configure(EntityTypeBuilder<Chequera> builder)
    {
        builder.ToTable("chequeras");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();
        builder.Property(x => x.CajaId).HasColumnName("caja_id").IsRequired();
        builder.Property(x => x.Banco).HasColumnName("banco").HasMaxLength(100).IsRequired();
        builder.Property(x => x.NroCuenta).HasColumnName("nro_cuenta").HasMaxLength(50).IsRequired();
        builder.Property(x => x.NroDesde).HasColumnName("nro_desde").IsRequired();
        builder.Property(x => x.NroHasta).HasColumnName("nro_hasta").IsRequired();
        builder.Property(x => x.UltimoChequeUsado).HasColumnName("ultimo_cheque_usado");
        builder.Property(x => x.Activa).HasColumnName("activa").HasDefaultValue(true);
        builder.Property(x => x.Observacion).HasColumnName("observacion");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");
        builder.Property(x => x.CreatedBy).HasColumnName("created_by");
        builder.Property(x => x.UpdatedBy).HasColumnName("updated_by");
        builder.HasIndex(x => x.CajaId);
        builder.HasIndex(x => x.Activa);
    }
}
