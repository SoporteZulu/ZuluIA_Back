using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZuluIA_Back.Domain.Entities.Precios;

namespace ZuluIA_Back.Infrastructure.Persistence.Configurations;

public class ListaPreciosConfiguration : IEntityTypeConfiguration<ListaPrecios>
{
    public void Configure(EntityTypeBuilder<ListaPrecios> builder)
    {
        builder.ToTable("listas_precios");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();

        builder.Property(x => x.Descripcion)
               .HasColumnName("descripcion")
               .HasMaxLength(200)
               .IsRequired();

        builder.Property(x => x.MonedaId)
               .HasColumnName("moneda_id")
               .IsRequired();

        builder.Property(x => x.VigenciaDesde)
               .HasColumnName("vigencia_desde");

        builder.Property(x => x.VigenciaHasta)
               .HasColumnName("vigencia_hasta");

        builder.Property(x => x.Activa)
               .HasColumnName("activa")
               .HasDefaultValue(true);

        builder.Property(x => x.EsPorDefecto)
               .HasColumnName("es_por_defecto")
               .HasDefaultValue(false);

        builder.Property(x => x.ListaPadreId)
               .HasColumnName("lista_padre_id");

        builder.Property(x => x.Prioridad)
               .HasColumnName("prioridad")
               .HasDefaultValue(0);

        builder.Property(x => x.Observaciones)
               .HasColumnName("observaciones")
               .HasMaxLength(500);

        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");
        builder.Property(x => x.CreatedBy).HasColumnName("created_by");
        builder.Property(x => x.UpdatedBy).HasColumnName("updated_by");

        builder.HasMany(x => x.Items)
               .WithOne()
               .HasForeignKey(x => x.ListaId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.ListaPadre)
               .WithMany()
               .HasForeignKey(x => x.ListaPadreId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.Activa);
        builder.HasIndex(x => x.EsPorDefecto);
        builder.HasIndex(x => x.MonedaId);
        builder.HasIndex(x => x.ListaPadreId);
        builder.HasIndex(x => new { x.VigenciaDesde, x.VigenciaHasta });
        builder.HasIndex(x => x.Prioridad);
    }
}