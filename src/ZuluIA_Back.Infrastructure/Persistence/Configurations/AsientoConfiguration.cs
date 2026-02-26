using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZuluIA_Back.Domain.Entities.Contabilidad;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Infrastructure.Persistence.Configurations;

public class AsientoConfiguration : IEntityTypeConfiguration<Asiento>
{
    public void Configure(EntityTypeBuilder<Asiento> builder)
    {
        builder.ToTable("asientos");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();
        builder.Property(x => x.EjercicioId).HasColumnName("ejercicio_id").IsRequired();
        builder.Property(x => x.SucursalId).HasColumnName("sucursal_id").IsRequired();
        builder.Property(x => x.Fecha).HasColumnName("fecha").IsRequired();
        builder.Property(x => x.Numero).HasColumnName("numero").IsRequired();
        builder.Property(x => x.Descripcion).HasColumnName("descripcion").HasMaxLength(500).IsRequired();
        builder.Property(x => x.OrigenTabla).HasColumnName("origen_tabla").HasMaxLength(50);
        builder.Property(x => x.OrigenId).HasColumnName("origen_id");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.CreatedBy).HasColumnName("created_by");
        builder.Property(x => x.UpdatedBy).HasColumnName("updated_by");

        builder.Property(x => x.Estado)
               .HasColumnName("estado")
               .HasConversion(
                   v => v.ToString(),
                   v => Enum.Parse<EstadoAsiento>(v, true))
               .HasMaxLength(20)
               .IsRequired();

        builder.HasMany(x => x.Lineas)
               .WithOne()
               .HasForeignKey(x => x.AsientoId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.EjercicioId, x.SucursalId, x.Numero }).IsUnique();
        builder.HasIndex(x => x.Fecha);
    }
}