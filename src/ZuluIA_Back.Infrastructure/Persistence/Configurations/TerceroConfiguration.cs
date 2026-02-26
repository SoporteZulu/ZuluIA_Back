using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZuluIA_Back.Domain.Entities.Terceros;
using ZuluIA_Back.Domain.ValueObjects;

namespace ZuluIA_Back.Infrastructure.Persistence.Configurations;

public class TerceroConfiguration : IEntityTypeConfiguration<Tercero>
{
    public void Configure(EntityTypeBuilder<Tercero> builder)
    {
        builder.ToTable("terceros");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();
        builder.Property(x => x.Legajo).HasColumnName("legajo").HasMaxLength(20).IsRequired();
        builder.Property(x => x.RazonSocial).HasColumnName("razon_social").HasMaxLength(200).IsRequired();
        builder.Property(x => x.NombreFantasia).HasColumnName("nombre_fantasia").HasMaxLength(200);
        builder.Property(x => x.TipoDocumentoId).HasColumnName("tipo_documento_id").IsRequired();
        builder.Property(x => x.NroDocumento).HasColumnName("nro_documento").HasMaxLength(30).IsRequired();
        builder.Property(x => x.CondicionIvaId).HasColumnName("condicion_iva_id").IsRequired();
        builder.Property(x => x.CategoriaId).HasColumnName("categoria_id");
        builder.Property(x => x.EsCliente).HasColumnName("es_cliente").HasDefaultValue(false);
        builder.Property(x => x.EsProveedor).HasColumnName("es_proveedor").HasDefaultValue(false);
        builder.Property(x => x.EsEmpleado).HasColumnName("es_empleado").HasDefaultValue(false);
        builder.Property(x => x.NroIngresosBrutos).HasColumnName("nro_ingresos_brutos").HasMaxLength(30);
        builder.Property(x => x.NroMunicipal).HasColumnName("nro_municipal").HasMaxLength(30);
        builder.Property(x => x.Telefono).HasColumnName("telefono").HasMaxLength(30);
        builder.Property(x => x.Celular).HasColumnName("celular").HasMaxLength(30);
        builder.Property(x => x.Email).HasColumnName("email").HasMaxLength(150);
        builder.Property(x => x.Web).HasColumnName("web").HasMaxLength(150);
        builder.Property(x => x.MonedaId).HasColumnName("moneda_id");
        builder.Property(x => x.LimiteCredito).HasColumnName("limite_credito").HasPrecision(18, 2);
        builder.Property(x => x.Facturable).HasColumnName("facturable").HasDefaultValue(true);
        builder.Property(x => x.SucursalId).HasColumnName("sucursal_id");
        builder.Property(x => x.Activo).HasColumnName("activo").HasDefaultValue(true);
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");
        builder.Property(x => x.CreatedBy).HasColumnName("created_by");
        builder.Property(x => x.UpdatedBy).HasColumnName("updated_by");

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

        builder.HasIndex(x => x.Legajo).IsUnique();
        builder.HasIndex(x => x.NroDocumento);
        builder.HasIndex(x => x.Activo);
    }
}