using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZuluIA_Back.Domain.Entities.Sucursales;

namespace ZuluIA_Back.Infrastructure.Persistence.Configurations;

public class SucursalConfiguration : IEntityTypeConfiguration<Sucursal>
{
    public void Configure(EntityTypeBuilder<Sucursal> builder)
    {
        builder.ToTable("sucursales");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();

        builder.Property(x => x.RazonSocial).HasColumnName("razon_social").HasMaxLength(200).IsRequired();
        builder.Property(x => x.NombreFantasia).HasColumnName("nombre_fantasia").HasMaxLength(200);
        builder.Property(x => x.Cuit).HasColumnName("cuit").HasMaxLength(20).IsRequired();
        builder.Property(x => x.NroIngresosBrutos).HasColumnName("nro_ingresos_brutos").HasMaxLength(30);
        builder.Property(x => x.CondicionIvaId).HasColumnName("condicion_iva_id").IsRequired();
        builder.Property(x => x.MonedaId).HasColumnName("moneda_id").IsRequired();
        builder.Property(x => x.PaisId).HasColumnName("pais_id").IsRequired();

        // ValueObject Domicilio (owned)
        builder.OwnsOne(x => x.Domicilio, d =>
        {
            d.Property(p => p.Calle).HasColumnName("calle").HasMaxLength(150);
            d.Property(p => p.Nro).HasColumnName("nro").HasMaxLength(20);
            d.Property(p => p.Piso).HasColumnName("piso").HasMaxLength(10);
            d.Property(p => p.Dpto).HasColumnName("dpto").HasMaxLength(10);
            d.Property(p => p.CodigoPostal).HasColumnName("codigo_postal").HasMaxLength(10);
            d.Property(p => p.LocalidadId).HasColumnName("localidad_id");
            d.Property(p => p.BarrioId).HasColumnName("barrio_id");
        });

        builder.Property(x => x.Telefono).HasColumnName("telefono").HasMaxLength(30);
        builder.Property(x => x.Email).HasColumnName("email").HasMaxLength(150);
        builder.Property(x => x.Web).HasColumnName("web").HasMaxLength(150);
        builder.Property(x => x.Cbu).HasColumnName("cbu").HasMaxLength(30);
        builder.Property(x => x.AliasCbu).HasColumnName("alias_cbu").HasMaxLength(50);
        builder.Property(x => x.Cai).HasColumnName("cai").HasMaxLength(30);
        builder.Property(x => x.PuertoAfip).HasColumnName("puerto_afip").HasDefaultValue((short)443);
        builder.Property(x => x.CasaMatriz).HasColumnName("casa_matriz").HasDefaultValue(false);
        builder.Property(x => x.Activa).HasColumnName("activa").HasDefaultValue(true);

        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");
        builder.Property(x => x.CreatedBy).HasColumnName("created_by");
        builder.Property(x => x.UpdatedBy).HasColumnName("updated_by");

        builder.HasIndex(x => x.Cuit);
        builder.HasIndex(x => x.Activa);
    }
}