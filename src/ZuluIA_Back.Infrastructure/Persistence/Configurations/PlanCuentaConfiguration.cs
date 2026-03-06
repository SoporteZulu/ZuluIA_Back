using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZuluIA_Back.Domain.Entities.Contabilidad;

namespace ZuluIA_Back.Infrastructure.Persistence.Configurations;

public class PlanCuentaConfiguration : IEntityTypeConfiguration<PlanCuenta>
{
    public void Configure(EntityTypeBuilder<PlanCuenta> builder)
    {
        builder.ToTable("plan_cuentas");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();

        builder.Property(x => x.EjercicioId)
               .HasColumnName("ejercicio_id")
               .IsRequired();

        builder.Property(x => x.IntegradoraId)
               .HasColumnName("integradora_id");

        builder.Property(x => x.CodigoCuenta)
               .HasColumnName("codigo_cuenta")
               .HasMaxLength(50)
               .IsRequired();

        builder.Property(x => x.Denominacion)
               .HasColumnName("denominacion")
               .HasMaxLength(300)
               .IsRequired();

        builder.Property(x => x.Nivel)
               .HasColumnName("nivel")
               .IsRequired();

        builder.Property(x => x.OrdenNivel)
               .HasColumnName("orden_nivel")
               .HasMaxLength(100)
               .IsRequired();

        builder.Property(x => x.Imputable)
               .HasColumnName("imputable")
               .HasDefaultValue(true);

        builder.Property(x => x.Tipo)
               .HasColumnName("tipo")
               .HasMaxLength(20);

        builder.Property(x => x.SaldoNormal)
               .HasColumnName("saldo_normal")
               .HasMaxLength(1);

        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");

        // Auto-referencia integradora → subcuentas
        builder.HasMany(x => x.Subcuentas)
               .WithOne()
               .HasForeignKey(x => x.IntegradoraId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.EjercicioId, x.CodigoCuenta }).IsUnique();
        builder.HasIndex(x => x.EjercicioId);
        builder.HasIndex(x => x.Imputable);
        builder.HasIndex(x => x.Nivel);
    }
}

public class PlanCuentaParametroConfiguration
    : IEntityTypeConfiguration<PlanCuentaParametro>
{
    public void Configure(EntityTypeBuilder<PlanCuentaParametro> builder)
    {
        builder.ToTable("plan_cuentas_parametros");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();

        builder.Property(x => x.EjercicioId)
               .HasColumnName("ejercicio_id")
               .IsRequired();

        builder.Property(x => x.CuentaId)
               .HasColumnName("cuenta_id")
               .IsRequired();

        builder.Property(x => x.Tabla)
               .HasColumnName("tabla")
               .HasMaxLength(100)
               .IsRequired();

        builder.Property(x => x.IdRegistro)
               .HasColumnName("id_registro")
               .IsRequired();

        builder.HasIndex(x => new { x.EjercicioId, x.Tabla, x.IdRegistro });
    }
}