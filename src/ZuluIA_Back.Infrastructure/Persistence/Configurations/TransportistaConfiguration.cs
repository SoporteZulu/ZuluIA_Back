using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZuluIA_Back.Domain.Entities.Extras;

namespace ZuluIA_Back.Infrastructure.Persistence.Configurations;

public class TransportistaConfiguration : IEntityTypeConfiguration<Transportista>
{
    public void Configure(EntityTypeBuilder<Transportista> builder)
    {
        builder.ToTable("transportistas");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();

        builder.Property(x => x.TerceroId)
               .HasColumnName("tercero_id")
               .IsRequired();

        builder.Property(x => x.NroCuitTransportista)
               .HasColumnName("nro_cuit_transportista")
               .HasMaxLength(20);

        builder.Ignore(x => x.DomicilioPartida);

        builder.Property(x => x.Patente)
               .HasColumnName("patente")
               .HasMaxLength(20);

        builder.Property(x => x.MarcaVehiculo)
               .HasColumnName("marca_vehiculo")
               .HasMaxLength(100);

        builder.Property(x => x.Activo)
               .HasColumnName("activo")
               .HasDefaultValue(true);

        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");

        builder.HasIndex(x => x.TerceroId).IsUnique();
        builder.HasIndex(x => x.Patente);
        builder.HasIndex(x => x.Activo);
    }
}

public class BusquedaConfiguration : IEntityTypeConfiguration<Busqueda>
{
    public void Configure(EntityTypeBuilder<Busqueda> builder)
    {
        builder.ToTable("busquedas");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();

        builder.Property(x => x.Nombre)
               .HasColumnName("nombre")
               .HasMaxLength(100)
               .IsRequired();

        builder.Property(x => x.Modulo)
               .HasColumnName("modulo")
               .HasMaxLength(50)
               .IsRequired();

        builder.Property(x => x.FiltrosJson)
               .HasColumnName("filtros_json")
               .HasColumnType("jsonb")
               .HasDefaultValue("{}");

        builder.Property(x => x.UsuarioId)
               .HasColumnName("usuario_id");

        builder.Property(x => x.EsGlobal)
               .HasColumnName("es_global")
               .HasDefaultValue(false);

        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");

        builder.HasIndex(x => new { x.UsuarioId, x.Modulo });
        builder.HasIndex(x => x.EsGlobal);
    }
}