using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZuluIA_Back.Domain.Entities.Terceros;
using ZuluIA_Back.Domain.ValueObjects;

namespace ZuluIA_Back.Infrastructure.Persistence.Configurations;

public class TerceroConfiguration : IEntityTypeConfiguration<Tercero>
{
    public void Configure(EntityTypeBuilder<Tercero> builder)
    {
        // ─── Tabla ────────────────────────────────────────────────────────────
        builder.ToTable("terceros");

        // ─── PK ───────────────────────────────────────────────────────────────
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
               .HasColumnName("id")
               .UseIdentityAlwaysColumn();          // bigint GENERATED ALWAYS AS IDENTITY

        // ─── Identificación ───────────────────────────────────────────────────
        builder.Property(x => x.Legajo)
               .HasColumnName("legajo")
               .HasMaxLength(20)
               .IsRequired();

        builder.Property(x => x.RazonSocial)
               .HasColumnName("razon_social")
               .HasMaxLength(200)
               .IsRequired();

        builder.Property(x => x.NombreFantasia)
               .HasColumnName("nombre_fantasia")
               .HasMaxLength(200);

        // ─── Documento e IVA ──────────────────────────────────────────────────
        builder.Property(x => x.TipoDocumentoId)
               .HasColumnName("tipo_documento_id")
               .IsRequired();

        builder.Property(x => x.NroDocumento)
               .HasColumnName("nro_documento")
               .HasMaxLength(30)
               .IsRequired();

        builder.Property(x => x.CondicionIvaId)
               .HasColumnName("condicion_iva_id")
               .IsRequired();

        // ─── Clasificación ────────────────────────────────────────────────────
        builder.Property(x => x.CategoriaId)
               .HasColumnName("categoria_id");

        builder.Property(x => x.EsCliente)
               .HasColumnName("es_cliente")
               .HasDefaultValue(false)
               .IsRequired();

        builder.Property(x => x.EsProveedor)
               .HasColumnName("es_proveedor")
               .HasDefaultValue(false)
               .IsRequired();

        builder.Property(x => x.EsEmpleado)
               .HasColumnName("es_empleado")
               .HasDefaultValue(false)
               .IsRequired();

        // ─── Domicilio (Value Object → columnas propias de la tabla) ──────────
        // UseSnakeCaseNamingConvention() ya convierte las propiedades,
        // pero se nombran explícitamente para garantizar el mapeo exacto
        // contra la tabla `terceros` de Supabase.
        builder.OwnsOne(x => x.Domicilio, d =>
        {
            d.Property(p => p.Calle)
             .HasColumnName("calle")
             .HasMaxLength(150);

            d.Property(p => p.Nro)
             .HasColumnName("nro")
             .HasMaxLength(20);

            d.Property(p => p.Piso)
             .HasColumnName("piso")
             .HasMaxLength(10);

            d.Property(p => p.Dpto)
             .HasColumnName("dpto")
             .HasMaxLength(10);

            d.Property(p => p.CodigoPostal)
             .HasColumnName("codigo_postal")
             .HasMaxLength(10);

            d.Property(p => p.LocalidadId)
             .HasColumnName("localidad_id");

            d.Property(p => p.BarrioId)
             .HasColumnName("barrio_id");
        });

        // ─── Datos fiscales ───────────────────────────────────────────────────
        builder.Property(x => x.NroIngresosBrutos)
               .HasColumnName("nro_ingresos_brutos")
               .HasMaxLength(30);

        builder.Property(x => x.NroMunicipal)
               .HasColumnName("nro_municipal")
               .HasMaxLength(30);

        // ─── Contacto ─────────────────────────────────────────────────────────
        builder.Property(x => x.Telefono)
               .HasColumnName("telefono")
               .HasMaxLength(30);

        builder.Property(x => x.Celular)
               .HasColumnName("celular")
               .HasMaxLength(30);

        builder.Property(x => x.Email)
               .HasColumnName("email")
               .HasMaxLength(150);

        builder.Property(x => x.Web)
               .HasColumnName("web")
               .HasMaxLength(150);

        // ─── Comercial ────────────────────────────────────────────────────────
        builder.Property(x => x.MonedaId)
               .HasColumnName("moneda_id");

        builder.Property(x => x.LimiteCredito)
               .HasColumnName("limite_credito")
               .HasPrecision(18, 2);

        builder.Property(x => x.Facturable)
               .HasColumnName("facturable")
               .HasDefaultValue(true)
               .IsRequired();

        builder.Property(x => x.CobradorId)
               .HasColumnName("cobrador_id");

        builder.Property(x => x.PctComisionCobrador)
               .HasColumnName("pct_comision_cobrador")
               .HasPrecision(5, 2)
               .HasDefaultValue(0m)
               .IsRequired();

        builder.Property(x => x.VendedorId)
               .HasColumnName("vendedor_id");

        builder.Property(x => x.PctComisionVendedor)
               .HasColumnName("pct_comision_vendedor")
               .HasPrecision(5, 2)
               .HasDefaultValue(0m)
               .IsRequired();

        builder.Property(x => x.Observacion)
               .HasColumnName("observacion")
               .HasColumnType("text");

        // ─── Control ──────────────────────────────────────────────────────────
        builder.Property(x => x.SucursalId)
               .HasColumnName("sucursal_id");

        builder.Property(x => x.Activo)
               .HasColumnName("activo")
               .HasDefaultValue(true)
               .IsRequired();

        // ─── Auditoría (AuditableEntity) ──────────────────────────────────────
        builder.Property(x => x.CreatedAt)
               .HasColumnName("created_at")
               .IsRequired();

        builder.Property(x => x.UpdatedAt)
               .HasColumnName("updated_at")
               .IsRequired();

        builder.Property(x => x.DeletedAt)
               .HasColumnName("deleted_at");

        builder.Property(x => x.CreatedBy)
               .HasColumnName("created_by");

        builder.Property(x => x.UpdatedBy)
               .HasColumnName("updated_by");

        // ─── Índices ──────────────────────────────────────────────────────────
        // Legajo: único y el más frecuente en búsqueda (ABM principal)
        builder.HasIndex(x => x.Legajo)
               .IsUnique()
               .HasDatabaseName("ix_terceros_legajo");

        // NroDocumento: búsqueda frecuente (validación de CUIT duplicado)
        builder.HasIndex(x => x.NroDocumento)
               .HasDatabaseName("ix_terceros_nro_documento");

        // Activo: casi todos los listados filtran por activo = true
        builder.HasIndex(x => x.Activo)
               .HasDatabaseName("ix_terceros_activo");

        // Email: búsqueda por email desde el módulo de notificaciones
        builder.HasIndex(x => x.Email)
               .HasDatabaseName("ix_terceros_email");

        // Índices compuestos para los filtros más usados del listado paginado
        builder.HasIndex(x => new { x.EsCliente, x.Activo })
               .HasDatabaseName("ix_terceros_es_cliente_activo");

        builder.HasIndex(x => new { x.EsProveedor, x.Activo })
               .HasDatabaseName("ix_terceros_es_proveedor_activo");

        builder.HasIndex(x => new { x.SucursalId, x.Activo })
               .HasDatabaseName("ix_terceros_sucursal_activo");

        builder.HasIndex(x => new { x.CondicionIvaId, x.Activo })
               .HasDatabaseName("ix_terceros_condicion_iva_activo");
    }
}