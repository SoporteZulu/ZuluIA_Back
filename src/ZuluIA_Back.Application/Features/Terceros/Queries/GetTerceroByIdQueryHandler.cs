using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ZuluIA_Back.Application.Features.Terceros.DTOs;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Interfaces;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Application.Features.Terceros.Queries;

public class GetTerceroByIdQueryHandler(
    ITerceroRepository repo,
    IApplicationDbContext db,
    IMapper mapper,
    ILogger<GetTerceroByIdQueryHandler> logger)
    : IRequestHandler<GetTerceroByIdQuery, Result<TerceroDto>>
{
    private sealed record CuentaContableInfo(long Id, string Codigo, string Descripcion);

    public async Task<Result<TerceroDto>> Handle(
        GetTerceroByIdQuery request,
        CancellationToken ct)
    {
        // ── 1. Obtener entidad ────────────────────────────────────────────────
        var tercero = await repo.GetByIdAsync(request.Id, ct);

        if (tercero is null)
            return Result.Failure<TerceroDto>(
                $"No se encontró el tercero con Id {request.Id}.");

        // ── 2. Mapeo base (campos escalares + Domicilio VO) ───────────────────
        var dto = mapper.Map<TerceroDto>(tercero);
        dto.FechaAlta = DateOnly.FromDateTime(tercero.CreatedAt.UtcDateTime);

        // ── 3. Resolver descripciones de catálogos ────────────────────────────

        // TipoDocumento
        var tipoDoc = await db.TiposDocumento
            .FindAsync(new object[] { tercero.TipoDocumentoId }, ct);
        dto.TipoDocumentoDescripcion = tipoDoc?.Descripcion ?? string.Empty;

        // CondiciónIVA
        var condIva = await db.CondicionesIva
            .FindAsync(new object[] { tercero.CondicionIvaId }, ct);
        dto.CondicionIvaDescripcion = condIva?.Descripcion ?? string.Empty;

        // Categoría (opcional)
        if (tercero.CategoriaId.HasValue)
        {
            var categoria = await db.CategoriasTerceros
                .FindAsync(new object[] { tercero.CategoriaId.Value }, ct);
            dto.CategoriaDescripcion = categoria?.Descripcion ?? string.Empty;
        }

        if (tercero.CategoriaClienteId.HasValue)
        {
            var categoriaCliente = await db.CategoriasClientes
                .FindAsync(new object[] { tercero.CategoriaClienteId.Value }, ct);
            dto.CategoriaClienteDescripcion = categoriaCliente?.Descripcion ?? string.Empty;
        }

        Domain.Entities.Terceros.EstadoCliente? estadoClienteEntity = null;
        if (tercero.EstadoClienteId.HasValue)
        {
            estadoClienteEntity = await db.EstadosClientes
                .FindAsync(new object[] { tercero.EstadoClienteId.Value }, ct);
            dto.EstadoClienteDescripcion = estadoClienteEntity?.Descripcion ?? string.Empty;
            dto.EstadoClienteBloquea = estadoClienteEntity?.Bloquea;
        }

        if (tercero.CategoriaProveedorId.HasValue)
        {
            var categoriaProveedor = await db.CategoriasProveedores
                .FindAsync(new object[] { tercero.CategoriaProveedorId.Value }, ct);
            dto.CategoriaProveedorDescripcion = categoriaProveedor?.Descripcion ?? string.Empty;
        }

        Domain.Entities.Terceros.EstadoProveedor? estadoProveedorEntity = null;
        if (tercero.EstadoProveedorId.HasValue)
        {
            estadoProveedorEntity = await db.EstadosProveedores
                .FindAsync(new object[] { tercero.EstadoProveedorId.Value }, ct);
            dto.EstadoProveedorDescripcion = estadoProveedorEntity?.Descripcion ?? string.Empty;
            dto.EstadoProveedorBloquea = estadoProveedorEntity?.Bloquea;
        }

        if (tercero.PaisId.HasValue)
        {
            var pais = await db.Paises
                .FindAsync(new object[] { tercero.PaisId.Value }, ct);
            dto.PaisDescripcion = pais?.Descripcion;
        }

        if (tercero.EstadoCivilId.HasValue)
        {
            var estadoCivil = await db.EstadosCiviles
                .FindAsync(new object[] { tercero.EstadoCivilId.Value }, ct);
            dto.EstadoCivilDescripcion = estadoCivil?.Descripcion ?? dto.EstadoCivil;
            dto.EstadoCivil = dto.EstadoCivilDescripcion;
        }

        if (tercero.EstadoPersonaId.HasValue)
        {
            dto.EstadoPersonaDescripcion = await db.EstadosPersonas
                .AsNoTracking()
                .Where(x => x.Id == tercero.EstadoPersonaId.Value)
                .Select(x => x.Descripcion)
                .FirstOrDefaultAsync(ct);
        }

        // Moneda (opcional)
        if (tercero.MonedaId.HasValue)
        {
            var moneda = await db.Monedas
                .FindAsync(new object[] { tercero.MonedaId.Value }, ct);
            dto.MonedaDescripcion = moneda?.Descripcion ?? string.Empty;
        }

        // Cobrador/Vendedor (opcional)
        if (tercero.CobradorId.HasValue)
        {
            var cobrador = await db.Usuarios
                .FindAsync(new object[] { tercero.CobradorId.Value }, ct);
            dto.CobradorUserName = cobrador?.UserName;
            dto.CobradorNombre = cobrador?.NombreCompleto ?? string.Empty;
        }
        if (tercero.VendedorId.HasValue)
        {
            var vendedor = await db.Usuarios
                .FindAsync(new object[] { tercero.VendedorId.Value }, ct);
            dto.VendedorUserName = vendedor?.UserName;
            dto.VendedorNombre = vendedor?.NombreCompleto ?? string.Empty;
        }

        dto.UsuarioCliente = await TerceroUsuarioClienteReadModelLoader.LoadAsync(db, tercero.UsuarioId, ct);
        dto.AccesoUsuarioCliente = dto.UsuarioCliente is not null;
        dto.UsuarioClienteUserName = dto.UsuarioCliente?.UserName;
        dto.UsuarioClienteGrupoUserName = dto.UsuarioCliente?.UsuarioGrupoUserName;
        dto.TieneUsuarioCliente = tercero.UsuarioId.HasValue;
        dto.UsuarioClienteActivo = dto.UsuarioCliente?.Activo ?? false;
        dto.MediosContacto = await TerceroMedioContactoReadModelLoader.LoadAsync(db, tercero.Id, logger, ct);

        string? estadoPersonaDescripcion = null;
        bool? estadoPersonaActivo = null;
        if (tercero.EstadoPersonaId.HasValue)
        {
            var estadoPersona = await db.EstadosPersonas
                .AsNoTracking()
                .Where(x => x.Id == tercero.EstadoPersonaId.Value && !x.IsDeleted)
                .Select(x => new { x.Descripcion, x.Activo })
                .FirstOrDefaultAsync(ct);

            estadoPersonaDescripcion = estadoPersona?.Descripcion;
            estadoPersonaActivo = estadoPersona?.Activo;
            dto.EstadoPersonaDescripcion = estadoPersonaDescripcion;
        }

        // Sucursal (opcional)
        if (tercero.SucursalId.HasValue)
        {
            var sucursal = await db.Sucursales
                .FindAsync(new object[] { tercero.SucursalId.Value }, ct);
            dto.SucursalDescripcion = sucursal?.RazonSocial ?? string.Empty;
        }

        var cuentaContable = await ResolveCuentaContableAsync(db, tercero, ct);
        dto.CuentaContableId = cuentaContable?.Id;
        dto.CuentaContableCodigo = cuentaContable?.Codigo;
        dto.CuentaContableDescripcion = cuentaContable?.Descripcion;

        var estadoOperativo = TerceroEstadoOperativoResolver.Resolve(
            tercero.Activo,
            estadoPersonaDescripcion,
            estadoPersonaActivo,
            tercero.EsCliente,
            estadoClienteEntity?.Descripcion,
            estadoClienteEntity?.Activo,
            estadoClienteEntity?.Bloquea,
            tercero.EsProveedor,
            estadoProveedorEntity?.Descripcion,
            estadoProveedorEntity?.Activo,
            estadoProveedorEntity?.Bloquea);

        dto.EstadoOperativo = estadoOperativo.Codigo;
        dto.EstadoOperativoDescripcion = estadoOperativo.Descripcion;
        dto.EstadoOperativoBloquea = estadoOperativo.Bloquea;
        dto.EstadoVisibleDescripcion = estadoOperativo.Descripcion;
        dto.EstadoVisibleBloquea = estadoOperativo.Bloquea;

        var perfil = await db.TercerosPerfilesComerciales
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TerceroId == tercero.Id && x.DeletedAt == null, ct);

        if (perfil is not null)
        {
            dto.PerfilComercial = mapper.Map<TerceroPerfilComercialDto>(perfil);

            if (perfil.ZonaComercialId.HasValue)
            {
                dto.PerfilComercial.ZonaComercialDescripcion = await db.ZonasComerciales
                    .AsNoTracking()
                    .Where(x => x.Id == perfil.ZonaComercialId.Value)
                    .Select(x => x.Descripcion)
                    .FirstOrDefaultAsync(ct);
            }

            dto.CuentaCorriente = TerceroCuentaCorrienteReadModelLoader.Load(tercero, perfil);
        }
        else
        {
            dto.PerfilComercial = new TerceroPerfilComercialDto
            {
                TerceroId = tercero.Id,
                RiesgoCrediticio = RiesgoCrediticioComercial.Normal.ToString().ToUpperInvariant()
            };

            dto.CuentaCorriente = TerceroCuentaCorrienteReadModelLoader.Load(tercero, null);
        }

        var domicilios = await db.PersonasDomicilios
            .AsNoTracking()
            .Where(x => x.PersonaId == tercero.Id)
            .OrderByDescending(x => x.EsDefecto)
            .ThenBy(x => x.Orden)
            .ThenBy(x => x.Calle)
            .ToListAsync(ct);

        var domiciliosDto = mapper.Map<List<TerceroDomicilioDto>>(domicilios);
        await TerceroDomicilioReadModelLoader.LoadDescripcionesAsync(db, domiciliosDto, ct);
        dto.Domicilios = domiciliosDto;

        var contactos = await db.TercerosContactos
            .AsNoTracking()
            .Where(x => x.TerceroId == tercero.Id && x.DeletedAt == null)
            .OrderByDescending(x => x.Principal)
            .ThenBy(x => x.Orden)
            .ThenBy(x => x.Nombre)
            .ToListAsync(ct);

        dto.Contactos = mapper.Map<IReadOnlyList<TerceroContactoDto>>(contactos);

        var sucursalesEntrega = await db.TercerosSucursalesEntrega
            .AsNoTracking()
            .Where(x => x.TerceroId == tercero.Id && x.DeletedAt == null)
            .OrderByDescending(x => x.Principal)
            .ThenBy(x => x.Orden)
            .ThenBy(x => x.Descripcion)
            .ToListAsync(ct);

        dto.SucursalesEntrega = mapper.Map<IReadOnlyList<TerceroSucursalEntregaDto>>(sucursalesEntrega);
        dto.SucursalEntregaPrincipal = dto.SucursalesEntrega.FirstOrDefault();
        dto.RequiereDefinirEntrega = tercero.EsCliente && dto.SucursalEntregaPrincipal is null;

        var transportes = await db.TercerosTransportes
            .AsNoTracking()
            .Where(x => x.TerceroId == tercero.Id && x.DeletedAt == null)
            .OrderByDescending(x => x.Principal)
            .ThenBy(x => x.Orden)
            .ThenBy(x => x.Nombre)
            .ToListAsync(ct);

        var transportesDto = mapper.Map<List<TerceroTransporteDto>>(transportes);
        var transportistaIds = transportes
            .Where(x => x.TransportistaId.HasValue)
            .Select(x => x.TransportistaId!.Value)
            .Distinct()
            .ToList();

        if (transportistaIds.Count > 0)
        {
            var nombresTransportistas = await db.Transportistas
                .AsNoTracking()
                .Where(x => transportistaIds.Contains(x.Id))
                .Join(db.Terceros.AsNoTracking(), t => t.TerceroId, ter => ter.Id, (t, ter) => new { t.Id, ter.RazonSocial })
                .ToDictionaryAsync(x => x.Id, x => x.RazonSocial, ct);

            for (var i = 0; i < transportesDto.Count; i++)
            {
                var source = transportes[i];
                if (source.TransportistaId.HasValue && nombresTransportistas.TryGetValue(source.TransportistaId.Value, out var nombre))
                    transportesDto[i].TransportistaNombre = nombre;
            }
        }

        dto.Transportes = transportesDto;

        var ventanasCobranza = await db.TercerosVentanasCobranza
            .AsNoTracking()
            .Where(x => x.TerceroId == tercero.Id && x.DeletedAt == null)
            .OrderByDescending(x => x.Principal)
            .ThenBy(x => x.Orden)
            .ThenBy(x => x.Dia)
            .ToListAsync(ct);

        dto.VentanasCobranza = mapper.Map<IReadOnlyList<TerceroVentanaCobranzaDto>>(ventanasCobranza);


        // ── 4. Resolver descripciones del Domicilio ───────────────────────────
        if (tercero.Domicilio.ProvinciaId.HasValue)
        {
            var provincia = await db.Provincias
                .FindAsync(new object[] { tercero.Domicilio.ProvinciaId.Value }, ct);
            dto.Domicilio.ProvinciaDescripcion = provincia?.Descripcion;
            dto.ProvinciaId = tercero.Domicilio.ProvinciaId;
            dto.ProvinciaDescripcion = dto.Domicilio.ProvinciaDescripcion;
        }

        if (tercero.Domicilio.LocalidadId.HasValue)
        {
            var localidad = await db.Localidades
                .AsNoTracking()
                .Where(x => x.Id == tercero.Domicilio.LocalidadId.Value)
                .Select(x => new { x.Descripcion, x.ProvinciaId })
                .FirstOrDefaultAsync(ct);
            dto.Domicilio.LocalidadDescripcion = localidad?.Descripcion;

            if (!dto.Domicilio.ProvinciaId.HasValue && localidad is not null)
            {
                dto.Domicilio.ProvinciaId = localidad.ProvinciaId;
                dto.ProvinciaId = localidad.ProvinciaId;

                var provincia = await db.Provincias
                    .FindAsync(new object[] { localidad.ProvinciaId }, ct);
                dto.Domicilio.ProvinciaDescripcion = provincia?.Descripcion;
                dto.ProvinciaDescripcion = dto.Domicilio.ProvinciaDescripcion;
            }
        }

        if (tercero.Domicilio.BarrioId.HasValue)
        {
            var barrio = await db.Barrios
                .FindAsync(new object[] { tercero.Domicilio.BarrioId.Value }, ct);
            dto.Domicilio.BarrioDescripcion = barrio?.Descripcion;
        }

        // Construir dirección completa formateada
        dto.Domicilio.Completo = ConstruirDireccionCompleta(
            tercero.Domicilio.Calle,
            tercero.Domicilio.Nro,
            tercero.Domicilio.Piso,
            tercero.Domicilio.Dpto,
            dto.Domicilio.LocalidadDescripcion,
            tercero.Domicilio.CodigoPostal);

        dto.Domicilio.GeografiaCompleta = TerceroUbicacionFormatter.BuildGeografiaCompleta(
            dto.Domicilio.ProvinciaDescripcion,
            dto.Domicilio.LocalidadDescripcion,
            dto.Domicilio.BarrioDescripcion);

        dto.Domicilio.UbicacionCompleta = TerceroUbicacionFormatter.BuildUbicacionCompleta(
            dto.Domicilio.Completo,
            dto.Domicilio.GeografiaCompleta,
            dto.Domicilio.CodigoPostal);

        dto.ProvinciaId ??= dto.Domicilio.ProvinciaId;
        dto.ProvinciaDescripcion ??= dto.Domicilio.ProvinciaDescripcion;
        dto.GeografiaCompleta = dto.Domicilio.GeografiaCompleta;
        dto.UbicacionCompleta = dto.Domicilio.UbicacionCompleta;

        return Result<TerceroDto>.Success(dto);
    }

    private static async Task<CuentaContableInfo?> ResolveCuentaContableAsync(
        IApplicationDbContext db,
        Domain.Entities.Terceros.Tercero tercero,
        CancellationToken ct)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var cuentasActivas =
            from parametro in db.PlanCuentasParametros.AsNoTracking()
            join cuenta in db.PlanCuentas.AsNoTracking() on parametro.CuentaId equals cuenta.Id
            join ejercicio in db.Ejercicios.AsNoTracking() on parametro.EjercicioId equals ejercicio.Id
            where parametro.IdRegistro == tercero.Id
                && parametro.Tabla == "personas"
                && ejercicio.FechaInicio <= today
                && ejercicio.FechaFin >= today
            select new
            {
                CuentaId = cuenta.Id,
                cuenta.CodigoCuenta,
                cuenta.Denominacion,
                parametro.EjercicioId
            };

        if (tercero.SucursalId.HasValue)
        {
            var cuentaPorSucursal = await (
                from cuenta in cuentasActivas
                join ejercicioSucursal in db.EjercicioSucursales.AsNoTracking() on cuenta.EjercicioId equals ejercicioSucursal.EjercicioId
                where ejercicioSucursal.SucursalId == tercero.SucursalId.Value
                    && ejercicioSucursal.UsaContabilidad
                orderby cuenta.CodigoCuenta
                select new CuentaContableInfo(cuenta.CuentaId, cuenta.CodigoCuenta, cuenta.Denominacion))
                .FirstOrDefaultAsync(ct);

            if (cuentaPorSucursal is not null)
                return cuentaPorSucursal;
        }

        return await cuentasActivas
            .OrderBy(x => x.CodigoCuenta)
            .Select(x => new CuentaContableInfo(x.CuentaId, x.CodigoCuenta, x.Denominacion))
            .FirstOrDefaultAsync(ct);
    }

    // ─── Helper: construir dirección formateada ───────────────────────────────
    private static string ConstruirDireccionCompleta(
        string? calle,
        string? nro,
        string? piso,
        string? dpto,
        string? localidad,
        string? codigoPostal)
    {
        var partes = new List<string>();

        var linea1 = new List<string>();
        if (!string.IsNullOrWhiteSpace(calle)) linea1.Add(calle!);
        if (!string.IsNullOrWhiteSpace(nro)) linea1.Add(nro!);
        if (!string.IsNullOrWhiteSpace(piso)) linea1.Add($"P{piso}");
        if (!string.IsNullOrWhiteSpace(dpto)) linea1.Add($"D{dpto}");
        if (linea1.Count > 0) partes.Add(string.Join(" ", linea1));

        var linea2 = new List<string>();
        if (!string.IsNullOrWhiteSpace(localidad)) linea2.Add(localidad!);
        if (!string.IsNullOrWhiteSpace(codigoPostal)) linea2.Add($"({codigoPostal})");
        if (linea2.Count > 0) partes.Add(string.Join(" ", linea2));

        return string.Join(", ", partes);
    }
}