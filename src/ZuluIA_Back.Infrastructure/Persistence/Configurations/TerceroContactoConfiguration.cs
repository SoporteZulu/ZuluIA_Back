using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZuluIA_Back.Domain.Entities.Terceros;

namespace ZuluIA_Back.Infrastructure.Persistence.Configurations;

public class TerceroContactoConfiguration : IEntityTypeConfiguration<TerceroContacto>
{
    public void Configure(EntityTypeBuilder<TerceroContacto> builder)
    {
        builder.ToTable("terceros_contactos");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .UseIdentityColumn();

        builder.Property(x => x.TerceroId)
            .HasColumnName("tercero_id")
            .IsRequired();

        builder.Property(x => x.Nombre)
            .HasColumnName("nombre")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Cargo)
            .HasColumnName("cargo")
            .HasMaxLength(100);

        builder.Property(x => x.Email)
            .HasColumnName("email")
            .HasMaxLength(150);

        builder.Property(x => x.Telefono)
            .HasColumnName("telefono")
            .HasMaxLength(50);

        builder.Property(x => x.Sector)
            .HasColumnName("sector")
            .HasMaxLength(100);

        builder.Property(x => x.Principal)
            .HasColumnName("principal")
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(x => x.Orden)
            .HasColumnName("orden")
            .HasDefaultValue(0)
            .IsRequired();

        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");
        builder.Property(x => x.CreatedBy).HasColumnName("created_by");
        builder.Property(x => x.UpdatedBy).HasColumnName("updated_by");

        builder.HasIndex(x => new { x.TerceroId, x.Principal })
            .HasDatabaseName("ix_terceros_contactos_tercero_principal");

        builder.HasIndex(x => new { x.TerceroId, x.Orden })
            .HasDatabaseName("ix_terceros_contactos_tercero_orden");

        builder.HasOne<Tercero>()
            .WithMany()
            .HasForeignKey(x => x.TerceroId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
