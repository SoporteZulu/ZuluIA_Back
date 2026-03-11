using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZuluIA_Back.Domain.Entities.Contabilidad;

namespace ZuluIA_Back.Infrastructure.Persistence.Configurations;

public class EjercicioConfiguration : IEntityTypeConfiguration<Ejercicio>
{
    public void Configure(EntityTypeBuilder<Ejercicio> builder)
    {
        builder.ToTable("ejercicios");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();

        builder.Property(x => x.Descripcion)
               .HasColumnName("descripcion")
               .HasMaxLength(200)
               .IsRequired();

        builder.Property(x => x.FechaInicio)
               .HasColumnName("fecha_inicio")
               .IsRequired();

        builder.Property(x => x.FechaFin)
               .HasColumnName("fecha_fin")
               .IsRequired();

        builder.Property(x => x.MascaraCuentaContable)
               .HasColumnName("mascara_cuenta_contable")
               .HasMaxLength(30)
               .HasDefaultValue("00.00.00.00");

        builder.Property(x => x.Cerrado)
               .HasColumnName("cerrado")
               .HasDefaultValue(false);

        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");

        builder.HasMany(x => x.Sucursales)
               .WithOne()
               .HasForeignKey(x => x.EjercicioId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.FechaInicio);
        builder.HasIndex(x => x.FechaFin);
        builder.HasIndex(x => x.Cerrado);
    }
}

public class EjercicioSucursalConfiguration : IEntityTypeConfiguration<EjercicioSucursal>
{
    public void Configure(EntityTypeBuilder<EjercicioSucursal> builder)
    {
        builder.ToTable("ejercicio_sucursal");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();

        builder.Property(x => x.EjercicioId)
               .HasColumnName("ejercicio_id")
               .IsRequired();

        builder.Property(x => x.SucursalId)
               .HasColumnName("sucursal_id")
               .IsRequired();

        builder.Property(x => x.UsaContabilidad)
               .HasColumnName("usa_contabilidad")
               .HasDefaultValue(true);

        builder.Property(x => x.CreatedAt)
               .HasColumnName("created_at");

        builder.HasIndex(x => new { x.EjercicioId, x.SucursalId }).IsUnique();
    }
}