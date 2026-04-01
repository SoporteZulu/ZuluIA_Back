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
#if NET8_0_OR_GREATER
               .UseIdentityAlwaysColumn(); // PostgreSQL: GENERATED ALWAYS AS IDENTITY
#else
               .UseIdentityColumn();
#endif

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

        builder.Property(x => x.TipoPersoneria)
               .HasColumnName("tipo_personeria")
               .HasConversion(v => v.ToString().ToUpperInvariant(), v => Enum.Parse<ZuluIA_Back.Domain.Enums.TipoPersoneriaTercero>(v, true))
               .HasMaxLength(20)
               .HasDefaultValue(ZuluIA_Back.Domain.Enums.TipoPersoneriaTercero.Juridica)
               .IsRequired();

        builder.Property(x => x.Nombre)
               .HasColumnName("nombre")
               .HasMaxLength(150);

        builder.Property(x => x.Apellido)
               .HasColumnName("apellido")
               .HasMaxLength(150);

        builder.Property(x => x.Tratamiento)
               .HasColumnName("tratamiento")
               .HasMaxLength(250);

        builder.Property(x => x.Profesion)
               .HasColumnName("profesion")
               .HasMaxLength(250);

        builder.Property(x => x.EstadoPersonaId)
               .HasColumnName("estado_persona_id");

        builder.Property(x => x.EstadoCivilId)
               .HasColumnName("estado_civil_id");

        builder.Property(x => x.EstadoCivil)
               .HasColumnName("estado_civil")
               .HasMaxLength(250);

        builder.Property(x => x.Nacionalidad)
               .HasColumnName("nacionalidad")
               .HasMaxLength(250);

        builder.Property(x => x.Sexo)
               .HasColumnName("sexo")
               .HasMaxLength(1);

        builder.Property(x => x.FechaNacimiento)
               .HasColumnName("fecha_nacimiento")
               .HasColumnType("date");

        builder.Property(x => x.FechaRegistro)
               .HasColumnName("fecha_registro")
               .HasColumnType("date");

        builder.Property(x => x.EsEntidadGubernamental)
               .HasColumnName("es_entidad_gubernamental")
               .HasDefaultValue(false)
               .IsRequired();

        builder.Property(x => x.ClaveFiscal)
               .HasColumnName("clave_fiscal")
               .HasMaxLength(50);

        builder.Property(x => x.ValorClaveFiscal)
               .HasColumnName("valor_clave_fiscal")
               .HasMaxLength(30);

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

        builder.Property(x => x.CategoriaClienteId)
               .HasColumnName("categoria_cliente_id");

        builder.Property(x => x.EstadoClienteId)
               .HasColumnName("estado_cliente_id");

        builder.Property(x => x.CategoriaProveedorId)
               .HasColumnName("categoria_proveedor_id");

        builder.Property(x => x.EstadoProveedorId)
               .HasColumnName("estado_proveedor_id");

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

            d.Property(p => p.ProvinciaId)
             .HasColumnName("provincia_id");

            d.Property(p => p.LocalidadId)
             .HasColumnName("localidad_id");

            d.Property(p => p.BarrioId)
             .HasColumnName("barrio_id");
        });

        builder.Property(x => x.PaisId)
               .HasColumnName("pais_id");

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

        builder.Property(x => x.UsuarioId)
               .HasColumnName("usuario_id");

        // ─── Comercial ────────────────────────────────────────────────────────
        builder.Property(x => x.MonedaId)
               .HasColumnName("moneda_id");

        builder.Property(x => x.LimiteCredito)
               .HasColumnName("limite_credito")
               .HasPrecision(18, 2);

        builder.Property(x => x.PorcentajeMaximoDescuento)
               .HasColumnName("porcentaje_maximo_descuento")
               .HasPrecision(5, 2);

        builder.Property(x => x.VigenciaCreditoDesde)
               .HasColumnName("vigencia_credito_desde")
               .HasColumnType("date");

        builder.Property(x => x.VigenciaCreditoHasta)
               .HasColumnName("vigencia_credito_hasta")
               .HasColumnType("date");

        builder.Property(x => x.Facturable)
               .HasColumnName("facturable")
               .HasDefaultValue(true)
               .IsRequired();

        builder.Property(x => x.CobradorId)
               .HasColumnName("cobrador_id");

        builder.Property(x => x.AplicaComisionCobrador)
               .HasColumnName("aplica_comision_cobrador")
               .HasDefaultValue(false)
               .IsRequired();

        builder.Property(x => x.PctComisionCobrador)
               .HasColumnName("pct_comision_cobrador")
               .HasPrecision(5, 2)
               .HasDefaultValue(0m)
               .IsRequired();

        builder.Property(x => x.VendedorId)
               .HasColumnName("vendedor_id");

        builder.Property(x => x.AplicaComisionVendedor)
               .HasColumnName("aplica_comision_vendedor")
               .HasDefaultValue(false)
               .IsRequired();

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
        builder.HasIndex(x => x.Legajo)
               .IsUnique()
               .HasDatabaseName("ix_terceros_legajo");

        builder.HasIndex(x => x.NroDocumento)
               .HasDatabaseName("ix_terceros_nro_documento");

        builder.HasIndex(x => x.Activo)
               .HasDatabaseName("ix_terceros_activo");

        builder.HasIndex(x => x.Email)
               .HasDatabaseName("ix_terceros_email");

        builder.HasIndex(x => new { x.EsCliente, x.Activo })
               .HasDatabaseName("ix_terceros_es_cliente_activo");

        builder.HasIndex(x => new { x.EsProveedor, x.Activo })
               .HasDatabaseName("ix_terceros_es_proveedor_activo");

        builder.HasIndex(x => new { x.SucursalId, x.Activo })
               .HasDatabaseName("ix_terceros_sucursal_activo");

        builder.HasIndex(x => new { x.CondicionIvaId, x.Activo })
               .HasDatabaseName("ix_terceros_condicion_iva_activo");

        builder.HasIndex(x => x.CategoriaClienteId)
               .HasDatabaseName("ix_terceros_categoria_cliente_id");

        builder.HasIndex(x => x.EstadoClienteId)
               .HasDatabaseName("ix_terceros_estado_cliente_id");

        builder.HasIndex(x => x.CategoriaProveedorId)
               .HasDatabaseName("ix_terceros_categoria_proveedor_id");

        builder.HasIndex(x => x.EstadoProveedorId)
               .HasDatabaseName("ix_terceros_estado_proveedor_id");

        builder.HasIndex(x => x.UsuarioId)
               .HasDatabaseName("ix_terceros_usuario_id");

        builder.HasIndex(x => x.EstadoCivilId)
               .HasDatabaseName("ix_terceros_estado_civil_id");

        builder.HasIndex(x => x.EstadoPersonaId)
               .HasDatabaseName("ix_terceros_estado_persona_id");

        builder.HasIndex(x => x.PaisId)
               .HasDatabaseName("ix_terceros_pais_id");

        builder.HasOne<EstadoPersonaCatalogo>()
               .WithMany()
               .HasForeignKey(x => x.EstadoPersonaId)
               .OnDelete(DeleteBehavior.Restrict)
               .HasConstraintName("fk_terceros_estados_personas_estado_persona_id");

        builder.HasOne<CategoriaCliente>()
               .WithMany()
               .HasForeignKey(x => x.CategoriaClienteId)
               .OnDelete(DeleteBehavior.Restrict)
               .HasConstraintName("fk_terceros_categorias_clientes_categoria_cliente_id");

        builder.HasOne<EstadoCliente>()
               .WithMany()
               .HasForeignKey(x => x.EstadoClienteId)
               .OnDelete(DeleteBehavior.Restrict)
               .HasConstraintName("fk_terceros_estados_clientes_estado_cliente_id");

        builder.HasOne<CategoriaProveedor>()
               .WithMany()
               .HasForeignKey(x => x.CategoriaProveedorId)
               .OnDelete(DeleteBehavior.Restrict)
               .HasConstraintName("fk_terceros_categorias_proveedores_categoria_proveedor_id");

        builder.HasOne<EstadoProveedor>()
               .WithMany()
               .HasForeignKey(x => x.EstadoProveedorId)
               .OnDelete(DeleteBehavior.Restrict)
               .HasConstraintName("fk_terceros_estados_proveedores_estado_proveedor_id");

        builder.HasOne<EstadoCivilCatalogo>()
               .WithMany()
               .HasForeignKey(x => x.EstadoCivilId)
               .OnDelete(DeleteBehavior.Restrict)
               .HasConstraintName("fk_terceros_estados_civiles_estado_civil_id");

        builder.HasOne<ZuluIA_Back.Domain.Entities.Geografia.Pais>()
               .WithMany()
               .HasForeignKey(x => x.PaisId)
               .OnDelete(DeleteBehavior.Restrict)
               .HasConstraintName("fk_terceros_paises_pais_id");

        builder.OwnsOne(x => x.Domicilio, d =>
        {
            d.HasIndex(p => p.ProvinciaId)
             .HasDatabaseName("ix_terceros_provincia_id");

            d.HasOne<ZuluIA_Back.Domain.Entities.Geografia.Provincia>()
             .WithMany()
             .HasForeignKey(p => p.ProvinciaId)
             .OnDelete(DeleteBehavior.Restrict)
             .HasConstraintName("fk_terceros_provincias_provincia_id");
        });

        builder.HasOne<ZuluIA_Back.Domain.Entities.Usuarios.Usuario>()
               .WithMany()
               .HasForeignKey(x => x.UsuarioId)
               .OnDelete(DeleteBehavior.Restrict)
               .HasConstraintName("fk_terceros_usuarios_usuario_id");
    }
}
