using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZuluIA_Back.Domain.Entities.Finanzas;

namespace ZuluIA_Back.Infrastructure.Persistence.Configurations;

public class CajaCuentaBancariaConfiguration : IEntityTypeConfiguration<CajaCuentaBancaria>
{
    public void Configure(EntityTypeBuilder<CajaCuentaBancaria> builder)
    {
        builder.ToTable("cajas_cuentas_bancarias");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();

        builder.Property(x => x.SucursalId)
               .HasColumnName("sucursal_id").IsRequired();

        builder.Property(x => x.TipoId)
               .HasColumnName("tipo_id").IsRequired();

        builder.Property(x => x.Descripcion)
               .HasColumnName("descripcion").HasMaxLength(200).IsRequired();

        builder.Property(x => x.MonedaId)
               .HasColumnName("moneda_id").IsRequired();

        builder.Property(x => x.Banco)
               .HasColumnName("banco").HasMaxLength(100);

        builder.Property(x => x.NroCuenta)
               .HasColumnName("nro_cuenta").HasMaxLength(50);

        builder.Property(x => x.Cbu)
               .HasColumnName("cbu").HasMaxLength(30);

        builder.Property(x => x.UsuarioId)
               .HasColumnName("usuario_id");

        builder.Property(x => x.NroCierreActual)
               .HasColumnName("nro_cierre_actual").HasDefaultValue(0);

        builder.Property(x => x.EsCaja)
               .HasColumnName("es_caja").HasDefaultValue(true);

        builder.Property(x => x.Activa)
               .HasColumnName("activa").HasDefaultValue(true);

        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");
        builder.Ignore(x => x.CreatedBy);
        builder.Ignore(x => x.UpdatedBy);

        builder.HasIndex(x => x.SucursalId);
        builder.HasIndex(x => x.UsuarioId);
        builder.HasIndex(x => x.Activa);
    }
}