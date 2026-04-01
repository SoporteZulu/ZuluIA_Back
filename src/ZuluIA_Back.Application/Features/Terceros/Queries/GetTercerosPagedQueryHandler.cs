using AutoMapper;
using MediatR;
using ZuluIA_Back.Application.Features.Terceros.DTOs;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Interfaces;
using ZuluIA_Back.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace ZuluIA_Back.Application.Features.Terceros.Queries;

public class GetTercerosPagedQueryHandler(
    ITerceroRepository repo,
    IApplicationDbContext db,
    IMapper mapper)
    : IRequestHandler<GetTercerosPagedQuery, PagedResult<TerceroListDto>>
{
    private sealed record UsuarioClienteResumen(string UserName, bool Activo);
    private sealed record UsuarioComercialResumen(string UserName, string Nombre);

    public async Task<PagedResult<TerceroListDto>> Handle(
        GetTercerosPagedQuery request,
        CancellationToken ct)
    {
        // ── 1. Validar y normalizar paginación ────────────────────────────────
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        // ── 2. Obtener página de entidades desde el repositorio ───────────────
        var paged = await repo.GetPagedAsync(
            page,
            pageSize,
            request.Search,
            request.SoloClientes,
            request.SoloProveedores,
            request.SoloEmpleados,
            request.SoloActivos,
            request.CondicionIvaId,
            request.CategoriaId,
            request.EstadoPersonaId,
            request.CategoriaClienteId,
            request.EstadoClienteId,
            request.CategoriaProveedorId,
            request.EstadoProveedorId,
            request.SucursalId,
            ct);

        if (paged.TotalCount == 0)
            return PagedResult<TerceroListDto>.Empty(page, pageSize);

        // ── 3. Mapeo base (campos escalares + RolDisplay via AfterMap) ─────────
        var items = mapper.Map<IReadOnlyList<TerceroListDto>>(paged.Items);

        // ── 4. Resolver descripciones de FK en batch ──────────────────────────
        // CondiciónIVA
        var condicionIvaIds = paged.Items
            .Select(x => x.CondicionIvaId)
            .Distinct()
            .ToList();

        Dictionary<long, string> condicionesIva = condicionIvaIds.Count > 0
            ? await db.CondicionesIva
                .Where(x => condicionIvaIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => x.Descripcion, ct)
            : [];

        var paisIds = paged.Items
            .Where(x => x.PaisId.HasValue)
            .Select(x => x.PaisId!.Value)
            .Distinct()
            .ToList();

        Dictionary<long, string> paises = paisIds.Count > 0
            ? await db.Paises
                .Where(x => paisIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => x.Descripcion, ct)
            : [];

        var categoriaIds = paged.Items
            .Where(x => x.CategoriaId.HasValue)
            .Select(x => x.CategoriaId!.Value)
            .Distinct()
            .ToList();

        Dictionary<long, string> categorias = categoriaIds.Count > 0
            ? await db.CategoriasTerceros
                .Where(x => categoriaIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => x.Descripcion, ct)
            : [];

        var monedaIds = paged.Items
            .Where(x => x.MonedaId.HasValue)
            .Select(x => x.MonedaId!.Value)
            .Distinct()
            .ToList();

        Dictionary<long, string> monedas = monedaIds.Count > 0
            ? await db.Monedas
                .Where(x => monedaIds.Contains(x.Id) && x.DeletedAt == null)
                .ToDictionaryAsync(x => x.Id, x => x.Descripcion, ct)
            : [];

        var sucursalIds = paged.Items
            .Where(x => x.SucursalId.HasValue)
            .Select(x => x.SucursalId!.Value)
            .Distinct()
            .ToList();

        Dictionary<long, string> sucursales = sucursalIds.Count > 0
            ? await db.Sucursales
                .Where(x => sucursalIds.Contains(x.Id) && x.DeletedAt == null)
                .ToDictionaryAsync(x => x.Id, x => x.RazonSocial, ct)
            : [];

        var tercerosIds = paged.Items
            .Select(x => x.Id)
            .Distinct()
            .ToList();

        Dictionary<long, (decimal? LimiteSaldo, DateOnly? VigenciaSaldoDesde, DateOnly? VigenciaSaldoHasta)> perfilesCuentaCorriente = tercerosIds.Count > 0
            ? await db.TercerosPerfilesComerciales
                .Where(x => tercerosIds.Contains(x.TerceroId) && x.DeletedAt == null)
                .ToDictionaryAsync(
                    x => x.TerceroId,
                    x => (x.SaldoMaximoVigente, x.VigenciaSaldoDesde, x.VigenciaSaldoHasta),
                    ct)
            : [];

        var sucursalesEntrega = tercerosIds.Count > 0
            ? await db.TercerosSucursalesEntrega
                .Where(x => tercerosIds.Contains(x.TerceroId) && x.DeletedAt == null)
                .OrderByDescending(x => x.Principal)
                .ThenBy(x => x.Orden)
                .ThenBy(x => x.Descripcion)
                .Select(x => new { x.TerceroId, x.Descripcion, x.Principal })
                .ToListAsync(ct)
            : [];

        var sucursalesEntregaPorTercero = sucursalesEntrega
            .GroupBy(x => x.TerceroId)
            .ToDictionary(
                x => x.Key,
                x => new
                {
                    TieneSucursales = x.Any(),
                    PrincipalDescripcion = x.Select(v => v.Descripcion).FirstOrDefault()
                });

        var usuarioIds = paged.Items
            .Where(x => x.UsuarioId.HasValue)
            .Select(x => x.UsuarioId!.Value)
            .Distinct()
            .ToList();

        var usuariosComercialesIds = paged.Items
            .Where(x => x.CobradorId.HasValue || x.VendedorId.HasValue)
            .SelectMany(x => new[] { x.CobradorId, x.VendedorId })
            .Where(x => x.HasValue)
            .Select(x => x!.Value)
            .Distinct()
            .ToList();

        Dictionary<long, UsuarioClienteResumen> usuariosCliente = usuarioIds.Count > 0
            ? await db.Usuarios
                .Where(x => usuarioIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => new UsuarioClienteResumen(x.UserName, x.Activo && x.DeletedAt == null), ct)
            : [];

        Dictionary<long, UsuarioComercialResumen> usuariosComerciales = usuariosComercialesIds.Count > 0
            ? await db.Usuarios
                .Where(x => usuariosComercialesIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => new UsuarioComercialResumen(x.UserName, x.NombreCompleto ?? x.UserName), ct)
            : [];

        Dictionary<long, string?> usuariosClienteGrupo = usuarioIds.Count > 0
            ? await db.UsuariosXUsuario
                .Where(x => usuarioIds.Contains(x.UsuarioMiembroId))
                .Join(
                    db.Usuarios,
                    rel => rel.UsuarioGrupoId,
                    usuarioGrupo => usuarioGrupo.Id,
                    (rel, usuarioGrupo) => new { rel.UsuarioMiembroId, usuarioGrupo.UserName })
                .GroupBy(x => x.UsuarioMiembroId)
                .ToDictionaryAsync(x => x.Key, x => x.Select(v => (string?)v.UserName).FirstOrDefault(), ct)
            : [];

        var estadosCivilesIds = paged.Items
            .Where(x => x.EstadoCivilId.HasValue)
            .Select(x => x.EstadoCivilId!.Value)
            .Distinct()
            .ToList();

        Dictionary<long, string> estadosCiviles = estadosCivilesIds.Count > 0
            ? await db.EstadosCiviles
                .Where(x => estadosCivilesIds.Contains(x.Id) && x.DeletedAt == null)
                .ToDictionaryAsync(x => x.Id, x => x.Descripcion, ct)
            : [];

        var estadosPersonasIds = paged.Items
            .Where(x => x.EstadoPersonaId.HasValue)
            .Select(x => x.EstadoPersonaId!.Value)
            .Distinct()
            .ToList();

        Dictionary<long, (string Descripcion, bool Activo)> estadosPersonas = estadosPersonasIds.Count > 0
            ? await db.EstadosPersonas
                .Where(x => estadosPersonasIds.Contains(x.Id) && x.DeletedAt == null)
                .ToDictionaryAsync(x => x.Id, x => (x.Descripcion, x.Activo), ct)
            : [];

        // Localidades (del domicilio, para columna de ubicación)
        var localidadIds = paged.Items
            .Where(x => x.Domicilio.LocalidadId.HasValue)
            .Select(x => x.Domicilio.LocalidadId!.Value)
            .Distinct()
            .ToList();

        Dictionary<long, (string Descripcion, long ProvinciaId)> localidades = localidadIds.Count > 0
            ? await db.Localidades
                .Where(x => localidadIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => (x.Descripcion, x.ProvinciaId), ct)
            : [];

        var barrioIds = paged.Items
            .Where(x => x.Domicilio.BarrioId.HasValue)
            .Select(x => x.Domicilio.BarrioId!.Value)
            .Distinct()
            .ToList();

        Dictionary<long, string> barrios = barrioIds.Count > 0
            ? await db.Barrios
                .Where(x => barrioIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => x.Descripcion, ct)
            : [];

        var provinciaIds = paged.Items
            .Where(x => x.Domicilio.ProvinciaId.HasValue)
            .Select(x => x.Domicilio.ProvinciaId!.Value)
            .Concat(localidades.Values.Select(x => x.ProvinciaId))
            .Distinct()
            .ToList();

        Dictionary<long, string> provincias = provinciaIds.Count > 0
            ? await db.Provincias
                .Where(x => provinciaIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => x.Descripcion, ct)
            : [];

        var categoriaClienteIds = paged.Items
            .Where(x => x.CategoriaClienteId.HasValue)
            .Select(x => x.CategoriaClienteId!.Value)
            .Distinct()
            .ToList();

        Dictionary<long, string> categoriasClientes = categoriaClienteIds.Count > 0
            ? await db.CategoriasClientes
                .Where(x => categoriaClienteIds.Contains(x.Id) && x.DeletedAt == null)
                .ToDictionaryAsync(x => x.Id, x => x.Descripcion, ct)
            : [];

        var estadoClienteIds = paged.Items
            .Where(x => x.EstadoClienteId.HasValue)
            .Select(x => x.EstadoClienteId!.Value)
            .Distinct()
            .ToList();

        Dictionary<long, (string Descripcion, bool Bloquea, bool Activo)> estadosClientes = estadoClienteIds.Count > 0
            ? await db.EstadosClientes
                .Where(x => estadoClienteIds.Contains(x.Id) && x.DeletedAt == null)
                .ToDictionaryAsync(x => x.Id, x => (x.Descripcion, x.Bloquea, x.Activo), ct)
            : [];

        var categoriaProveedorIds = paged.Items
            .Where(x => x.CategoriaProveedorId.HasValue)
            .Select(x => x.CategoriaProveedorId!.Value)
            .Distinct()
            .ToList();

        Dictionary<long, string> categoriasProveedores = categoriaProveedorIds.Count > 0
            ? await db.CategoriasProveedores
                .Where(x => categoriaProveedorIds.Contains(x.Id) && x.DeletedAt == null)
                .ToDictionaryAsync(x => x.Id, x => x.Descripcion, ct)
            : [];

        var estadoProveedorIds = paged.Items
            .Where(x => x.EstadoProveedorId.HasValue)
            .Select(x => x.EstadoProveedorId!.Value)
            .Distinct()
            .ToList();

        Dictionary<long, (string Descripcion, bool Bloquea, bool Activo)> estadosProveedores = estadoProveedorIds.Count > 0
            ? await db.EstadosProveedores
                .Where(x => estadoProveedorIds.Contains(x.Id) && x.DeletedAt == null)
                .ToDictionaryAsync(x => x.Id, x => (x.Descripcion, x.Bloquea, x.Activo), ct)
            : [];

        // ── 5. Enriquecer los DTOs con las descripciones ──────────────────────
        // Iteramos sobre los ítems mapeados y los originales en paralelo
        // para poder acceder a las FK del dominio sin romper el DTO.
        var itemsList = items.ToList();

        for (var i = 0; i < itemsList.Count; i++)
        {
            var dto = itemsList[i];
            var tercero = paged.Items[i];

            dto.FechaAlta = DateOnly.FromDateTime(tercero.CreatedAt.UtcDateTime);

            dto.CondicionIvaDescripcion =
                condicionesIva.TryGetValue(tercero.CondicionIvaId, out var civa)
                ? civa : string.Empty;

            if (tercero.PaisId.HasValue &&
                paises.TryGetValue(tercero.PaisId.Value, out var pais))
                dto.PaisDescripcion = pais;

            if (tercero.CategoriaId.HasValue &&
                categorias.TryGetValue(tercero.CategoriaId.Value, out var categoriaDescripcion))
                dto.CategoriaDescripcion = categoriaDescripcion;

            if (tercero.MonedaId.HasValue &&
                monedas.TryGetValue(tercero.MonedaId.Value, out var monedaDescripcion))
                dto.MonedaDescripcion = monedaDescripcion;

            if (tercero.SucursalId.HasValue &&
                sucursales.TryGetValue(tercero.SucursalId.Value, out var sucursalDescripcion))
                dto.SucursalDescripcion = sucursalDescripcion;

            dto.TieneUsuarioCliente = tercero.UsuarioId.HasValue;
            dto.AccesoUsuarioCliente = dto.TieneUsuarioCliente;

            if (tercero.UsuarioId.HasValue &&
                usuariosCliente.TryGetValue(tercero.UsuarioId.Value, out var usuarioCliente))
            {
                dto.UsuarioClienteActivo = usuarioCliente.Activo;
                dto.UsuarioClienteUserName = usuarioCliente.UserName;
            }

            if (tercero.UsuarioId.HasValue &&
                usuariosClienteGrupo.TryGetValue(tercero.UsuarioId.Value, out var usuarioGrupoUserName))
                dto.UsuarioClienteGrupoUserName = usuarioGrupoUserName;

            if (tercero.CobradorId.HasValue &&
                usuariosComerciales.TryGetValue(tercero.CobradorId.Value, out var cobrador))
            {
                dto.CobradorUserName = cobrador.UserName;
                dto.CobradorNombre = cobrador.Nombre;
            }

            if (tercero.VendedorId.HasValue &&
                usuariosComerciales.TryGetValue(tercero.VendedorId.Value, out var vendedor))
            {
                dto.VendedorUserName = vendedor.UserName;
                dto.VendedorNombre = vendedor.Nombre;
            }

            if (perfilesCuentaCorriente.TryGetValue(tercero.Id, out var cuentaCorriente))
            {
                dto.LimiteSaldo = cuentaCorriente.LimiteSaldo;
                dto.VigenciaLimiteSaldoDesde = cuentaCorriente.VigenciaSaldoDesde;
                dto.VigenciaLimiteSaldoHasta = cuentaCorriente.VigenciaSaldoHasta;
            }

            dto.LimiteCredito = tercero.LimiteCredito;
            dto.VigenciaCreditoDesde = tercero.VigenciaCreditoDesde;
            dto.VigenciaCreditoHasta = tercero.VigenciaCreditoHasta;

            dto.LimiteCreditoResumen = BuildImporteResumen(dto.LimiteCredito);
            dto.LimiteSaldoResumen = BuildImporteResumen(dto.LimiteSaldo);
            dto.VigenciaCreditoResumen = BuildVigenciaResumen(dto.VigenciaCreditoDesde, dto.VigenciaCreditoHasta);
            dto.VigenciaLimiteSaldoResumen = BuildVigenciaResumen(dto.VigenciaLimiteSaldoDesde, dto.VigenciaLimiteSaldoHasta);

            if (tercero.EstadoCivilId.HasValue &&
                estadosCiviles.TryGetValue(tercero.EstadoCivilId.Value, out var estadoCivilDescripcion))
                dto.EstadoCivilDescripcion = estadoCivilDescripcion;

            if (tercero.EstadoPersonaId.HasValue &&
                estadosPersonas.TryGetValue(tercero.EstadoPersonaId.Value, out var estadoPersonaDescripcion))
                dto.EstadoPersonaDescripcion = estadoPersonaDescripcion.Descripcion;

            long? provinciaId = tercero.Domicilio.ProvinciaId;

            if (tercero.Domicilio.LocalidadId.HasValue &&
                localidades.TryGetValue(tercero.Domicilio.LocalidadId.Value, out var loc))
            {
                dto.LocalidadDescripcion = loc.Descripcion;
                provinciaId ??= loc.ProvinciaId;
            }

            if (tercero.Domicilio.BarrioId.HasValue &&
                barrios.TryGetValue(tercero.Domicilio.BarrioId.Value, out var barrioDescripcion))
                dto.BarrioDescripcion = barrioDescripcion;

            dto.ProvinciaId = provinciaId;

            if (provinciaId.HasValue && provincias.TryGetValue(provinciaId.Value, out var provinciaDescripcion))
                dto.ProvinciaDescripcion = provinciaDescripcion;

            var direccionCompleta = tercero.Domicilio.Completo;
            dto.GeografiaCompleta = TerceroUbicacionFormatter.BuildGeografiaCompleta(
                dto.ProvinciaDescripcion,
                dto.LocalidadDescripcion,
                dto.BarrioDescripcion);
            dto.UbicacionCompleta = TerceroUbicacionFormatter.BuildUbicacionCompleta(
                direccionCompleta,
                dto.GeografiaCompleta,
                dto.CodigoPostal);

            if (sucursalesEntregaPorTercero.TryGetValue(tercero.Id, out var sucursalEntrega))
            {
                dto.TieneSucursalesEntrega = sucursalEntrega.TieneSucursales;
                dto.SucursalEntregaPrincipalDescripcion = sucursalEntrega.PrincipalDescripcion;
            }

            dto.RequiereDefinirEntrega = tercero.EsCliente && !dto.TieneSucursalesEntrega;

            if (tercero.CategoriaClienteId.HasValue &&
                categoriasClientes.TryGetValue(tercero.CategoriaClienteId.Value, out var categoriaCliente))
                dto.CategoriaClienteDescripcion = categoriaCliente;

            if (tercero.EstadoClienteId.HasValue &&
                estadosClientes.TryGetValue(tercero.EstadoClienteId.Value, out var estadoCliente))
            {
                dto.EstadoClienteDescripcion = estadoCliente.Descripcion;
                dto.EstadoClienteBloquea = estadoCliente.Bloquea;
            }

            if (tercero.CategoriaProveedorId.HasValue &&
                categoriasProveedores.TryGetValue(tercero.CategoriaProveedorId.Value, out var categoriaProveedor))
                dto.CategoriaProveedorDescripcion = categoriaProveedor;

            if (tercero.EstadoProveedorId.HasValue &&
                estadosProveedores.TryGetValue(tercero.EstadoProveedorId.Value, out var estadoProveedor))
            {
                dto.EstadoProveedorDescripcion = estadoProveedor.Descripcion;
                dto.EstadoProveedorBloquea = estadoProveedor.Bloquea;
            }

            var estadoClienteDescripcion = dto.EstadoClienteDescripcion;
            var estadoClienteActivo = tercero.EstadoClienteId.HasValue && estadosClientes.TryGetValue(tercero.EstadoClienteId.Value, out var estadoClienteActual)
                ? estadoClienteActual.Activo
                : (bool?)null;
            var estadoClienteBloquea = dto.EstadoClienteBloquea;
            var estadoPersonaDescripcionActual = tercero.EstadoPersonaId.HasValue && estadosPersonas.TryGetValue(tercero.EstadoPersonaId.Value, out var estadoPersonaActual)
                ? estadoPersonaActual.Descripcion
                : null;
            var estadoPersonaActivo = tercero.EstadoPersonaId.HasValue && estadosPersonas.TryGetValue(tercero.EstadoPersonaId.Value, out estadoPersonaActual)
                ? estadoPersonaActual.Activo
                : (bool?)null;
            var estadoProveedorDescripcion = dto.EstadoProveedorDescripcion;
            var estadoProveedorActivo = tercero.EstadoProveedorId.HasValue && estadosProveedores.TryGetValue(tercero.EstadoProveedorId.Value, out var estadoProveedorActual)
                ? estadoProveedorActual.Activo
                : (bool?)null;
            var estadoProveedorBloquea = dto.EstadoProveedorBloquea;

            var estadoOperativo = TerceroEstadoOperativoResolver.Resolve(
                tercero.Activo,
                estadoPersonaDescripcionActual,
                estadoPersonaActivo,
                tercero.EsCliente,
                estadoClienteDescripcion,
                estadoClienteActivo,
                estadoClienteBloquea,
                tercero.EsProveedor,
                estadoProveedorDescripcion,
                estadoProveedorActivo,
                estadoProveedorBloquea);

            dto.EstadoOperativo = estadoOperativo.Codigo;
            dto.EstadoOperativoDescripcion = estadoOperativo.Descripcion;
            dto.EstadoOperativoBloquea = estadoOperativo.Bloquea;
            dto.EstadoVisibleDescripcion = estadoOperativo.Descripcion;
            dto.EstadoVisibleBloquea = estadoOperativo.Bloquea;

            // Validar si puede vender
            dto.PuedeVender = ValidarPuedeVender(tercero, estadoClienteActivo, estadoClienteBloquea, estadoPersonaActivo);
            dto.MotivoBloqueoVentas = !dto.PuedeVender
                ? ResolverMotivoBloqueoVentas(tercero, estadoClienteDescripcion, estadoClienteActivo, estadoClienteBloquea, estadoPersonaDescripcionActual, estadoPersonaActivo)
                : null;

            // Validar si puede comprar
            dto.PuedeComprar = ValidarPuedeComprar(tercero, estadoProveedorActivo, estadoProveedorBloquea, estadoPersonaActivo);
            dto.MotivoBloqueoCompras = !dto.PuedeComprar
                ? ResolverMotivoBloqueoCompras(tercero, estadoProveedorDescripcion, estadoProveedorActivo, estadoProveedorBloquea, estadoPersonaDescripcionActual, estadoPersonaActivo)
                : null;
        }

        return new PagedResult<TerceroListDto>(itemsList, page, pageSize, paged.TotalCount);
    }

    private static string? BuildImporteResumen(decimal? importe)
        => importe.HasValue
            ? importe.Value.ToString("0.##", CultureInfo.InvariantCulture)
            : null;

    private static string? BuildVigenciaResumen(DateOnly? desde, DateOnly? hasta)
    {
        if (!desde.HasValue && !hasta.HasValue)
            return null;

        if (desde.HasValue && hasta.HasValue)
            return $"{desde:yyyy-MM-dd} a {hasta:yyyy-MM-dd}";

        if (desde.HasValue)
            return $"Desde {desde:yyyy-MM-dd}";

        return $"Hasta {hasta:yyyy-MM-dd}";
    }

    private static bool ValidarPuedeVender(
        Domain.Entities.Terceros.Tercero tercero,
        bool? estadoClienteActivo,
        bool? estadoClienteBloquea,
        bool? estadoPersonaActivo)
    {
        if (!tercero.EsCliente || !tercero.Activo)
            return false;

        if (estadoPersonaActivo.HasValue && !estadoPersonaActivo.Value)
            return false;

        if (estadoClienteActivo.HasValue && !estadoClienteActivo.Value)
            return false;

        if (estadoClienteBloquea.HasValue && estadoClienteBloquea.Value)
            return false;

        if (!tercero.Facturable)
            return false;

        return true;
    }

    private static string ResolverMotivoBloqueoVentas(
        Domain.Entities.Terceros.Tercero tercero,
        string? estadoClienteDescripcion,
        bool? estadoClienteActivo,
        bool? estadoClienteBloquea,
        string? estadoPersonaDescripcion,
        bool? estadoPersonaActivo)
    {
        if (!tercero.EsCliente)
            return "No es cliente";

        if (!tercero.Activo)
            return "Cliente inactivo";

        if (estadoPersonaActivo.HasValue && !estadoPersonaActivo.Value && !string.IsNullOrWhiteSpace(estadoPersonaDescripcion))
            return $"Estado de persona: {estadoPersonaDescripcion}";

        if (estadoClienteActivo.HasValue && !estadoClienteActivo.Value && !string.IsNullOrWhiteSpace(estadoClienteDescripcion))
            return $"Estado de cliente inactivo: {estadoClienteDescripcion}";

        if (estadoClienteBloquea.HasValue && estadoClienteBloquea.Value && !string.IsNullOrWhiteSpace(estadoClienteDescripcion))
            return $"Cliente bloqueado: {estadoClienteDescripcion}";

        if (!tercero.Facturable)
            return "Cliente no facturable";

        return "Cliente no operativo";
    }

    private static bool ValidarPuedeComprar(
        Domain.Entities.Terceros.Tercero tercero,
        bool? estadoProveedorActivo,
        bool? estadoProveedorBloquea,
        bool? estadoPersonaActivo)
    {
        if (!tercero.EsProveedor || !tercero.Activo)
            return false;

        if (estadoPersonaActivo.HasValue && !estadoPersonaActivo.Value)
            return false;

        if (estadoProveedorActivo.HasValue && !estadoProveedorActivo.Value)
            return false;

        if (estadoProveedorBloquea.HasValue && estadoProveedorBloquea.Value)
            return false;

        return true;
    }

    private static string ResolverMotivoBloqueoCompras(
        Domain.Entities.Terceros.Tercero tercero,
        string? estadoProveedorDescripcion,
        bool? estadoProveedorActivo,
        bool? estadoProveedorBloquea,
        string? estadoPersonaDescripcion,
        bool? estadoPersonaActivo)
    {
        if (!tercero.EsProveedor)
            return "No es proveedor";

        if (!tercero.Activo)
            return "Proveedor inactivo";

        if (estadoPersonaActivo.HasValue && !estadoPersonaActivo.Value && !string.IsNullOrWhiteSpace(estadoPersonaDescripcion))
            return $"Estado de persona: {estadoPersonaDescripcion}";

        if (estadoProveedorActivo.HasValue && !estadoProveedorActivo.Value && !string.IsNullOrWhiteSpace(estadoProveedorDescripcion))
            return $"Estado de proveedor inactivo: {estadoProveedorDescripcion}";

        if (estadoProveedorBloquea.HasValue && estadoProveedorBloquea.Value && !string.IsNullOrWhiteSpace(estadoProveedorDescripcion))
            return $"Proveedor bloqueado: {estadoProveedorDescripcion}";

        return "Proveedor no operativo";
    }
}