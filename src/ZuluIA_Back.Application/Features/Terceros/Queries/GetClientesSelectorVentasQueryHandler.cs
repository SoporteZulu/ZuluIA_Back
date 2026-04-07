using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Terceros.DTOs;
using ZuluIA_Back.Domain.Interfaces;

#pragma warning disable CS9113
namespace ZuluIA_Back.Application.Features.Terceros.Queries;

public class GetClientesSelectorVentasQueryHandler(
    ITerceroRepository _repo,
    IApplicationDbContext db,
    IMapper _mapper)
    : IRequestHandler<GetClientesSelectorVentasQuery, IReadOnlyList<ClienteSelectorVentasDto>>
{
    private const int MaxTake = 100;

    public async Task<IReadOnlyList<ClienteSelectorVentasDto>> Handle(
        GetClientesSelectorVentasQuery request,
        CancellationToken ct)
    {
        var maxResults = Math.Clamp(request.MaxResults, 1, MaxTake);

        // ── 1. Obtener clientes activos filtrados ─────────────────────────────
        var query = db.Terceros
            .AsNoTracking()
            .Where(x => x.EsCliente && x.Activo && x.DeletedAt == null);

        // Filtro por sucursal
        if (request.SucursalId.HasValue)
            query = query.Where(x => x.SucursalId == request.SucursalId.Value
                                  || x.SucursalId == null);

        // Búsqueda textual
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim().ToLowerInvariant();
            var normalizedTerm = NormalizeSearchTerm(term);

            query = query.Where(x =>
                x.RazonSocial.ToLower().Contains(term) ||
                x.Legajo.ToLower().Contains(term) ||
                x.NroDocumento.ToLower().Contains(term) ||
                x.NroDocumento
                    .Replace("-", string.Empty)
                    .Replace(".", string.Empty)
                    .Replace("/", string.Empty)
                    .Replace(" ", string.Empty)
                    .ToLower()
                    .Contains(normalizedTerm) ||
                (x.NombreFantasia != null && x.NombreFantasia.ToLower().Contains(term)));
        }

        var terceros = await query
            .OrderBy(x => x.RazonSocial)
            .Take(maxResults)
            .ToListAsync(ct);

        if (terceros.Count == 0)
            return [];

        // ── 2. Resolver descripciones de FK en batch ──────────────────────────
        var terceroIds = terceros.Select(x => x.Id).Distinct().ToList();
        
        var condicionIvaIds = terceros.Select(x => x.CondicionIvaId).Distinct().ToList();
        var condicionesIva = await db.CondicionesIva
            .Where(x => condicionIvaIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, x => x.Descripcion, ct);

        var monedaIds = terceros
            .Where(x => x.MonedaId.HasValue)
            .Select(x => x.MonedaId!.Value)
            .Distinct()
            .ToList();
        var monedas = monedaIds.Count > 0
            ? await db.Monedas
                .Where(x => monedaIds.Contains(x.Id) && x.DeletedAt == null)
                .ToDictionaryAsync(x => x.Id, x => x.Descripcion, ct)
            : new Dictionary<long, string>();

        var vendedorIds = terceros
            .Where(x => x.VendedorId.HasValue)
            .Select(x => x.VendedorId!.Value)
            .Distinct()
            .ToList();
        var vendedores = vendedorIds.Count > 0
            ? await db.Usuarios
                .Where(x => vendedorIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => x.NombreCompleto ?? x.UserName, ct)
            : new Dictionary<long, string>();

        var estadoClienteIds = terceros
            .Where(x => x.EstadoClienteId.HasValue)
            .Select(x => x.EstadoClienteId!.Value)
            .Distinct()
            .ToList();
        var estadosClientes = new Dictionary<long, (string Descripcion, bool Bloquea, bool Activo)>();
        if (estadoClienteIds.Count > 0)
        {
            estadosClientes = await db.EstadosClientes
                .Where(x => estadoClienteIds.Contains(x.Id) && x.DeletedAt == null)
                .ToDictionaryAsync(x => x.Id, x => (x.Descripcion, x.Bloquea, x.Activo), ct);
        }

        var estadoPersonaIds = terceros
            .Where(x => x.EstadoPersonaId.HasValue)
            .Select(x => x.EstadoPersonaId!.Value)
            .Distinct()
            .ToList();
        var estadosPersonas = new Dictionary<long, (string Descripcion, bool Activo)>();
        if (estadoPersonaIds.Count > 0)
        {
            estadosPersonas = await db.EstadosPersonas
                .Where(x => estadoPersonaIds.Contains(x.Id) && x.DeletedAt == null)
                .ToDictionaryAsync(x => x.Id, x => (x.Descripcion, x.Activo), ct);
        }

        // Cargar geografía para ubicación
        var localidadIds = terceros
            .Where(x => x.Domicilio.LocalidadId.HasValue)
            .Select(x => x.Domicilio.LocalidadId!.Value)
            .Distinct()
            .ToList();
        var localidades = new Dictionary<long, (string Descripcion, long ProvinciaId)>();
        if (localidadIds.Count > 0)
        {
            localidades = await db.Localidades
                .Where(x => localidadIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => (x.Descripcion, x.ProvinciaId), ct);
        }

        var provinciaIds = terceros
            .Where(x => x.Domicilio.ProvinciaId.HasValue)
            .Select(x => x.Domicilio.ProvinciaId!.Value)
            .Concat(localidades.Values.Select(x => x.ProvinciaId))
            .Distinct()
            .ToList();
        var provincias = provinciaIds.Count > 0
            ? await db.Provincias
                .Where(x => provinciaIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => x.Descripcion, ct)
            : new Dictionary<long, string>();

        var barrioIds = terceros
            .Where(x => x.Domicilio.BarrioId.HasValue)
            .Select(x => x.Domicilio.BarrioId!.Value)
            .Distinct()
            .ToList();
        var barrios = barrioIds.Count > 0
            ? await db.Barrios
                .Where(x => barrioIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => x.Descripcion, ct)
            : new Dictionary<long, string>();

        var sucursalesEntrega = terceroIds.Count > 0
            ? await db.TercerosSucursalesEntrega
                .Where(x => terceroIds.Contains(x.TerceroId) && x.DeletedAt == null)
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

        var perfilesComerciales = terceroIds.Count > 0
            ? await db.TercerosPerfilesComerciales
                .AsNoTracking()
                .Where(x => terceroIds.Contains(x.TerceroId) && x.DeletedAt == null)
                .ToListAsync(ct)
            : [];

        var perfilesPorTercero = perfilesComerciales
            .GroupBy(x => x.TerceroId)
            .ToDictionary(
                x => x.Key,
                x => new
                {
                    TienePerfil = true,
                    RiesgoCrediticio = x.Select(v => v.RiesgoCrediticio.ToString().ToUpperInvariant()).FirstOrDefault() ?? "NORMAL",
                    SaldoMaximoVigente = x.Select(v => v.SaldoMaximoVigente).FirstOrDefault(),
                    CondicionCobranza = x.Select(v => v.CondicionCobranza).FirstOrDefault(),
                    CondicionVenta = x.Select(v => v.CondicionVenta).FirstOrDefault(),
                    PlazoCobro = x.Select(v => v.PlazoCobro).FirstOrDefault(),
                    FacturadorPorDefecto = x.Select(v => v.FacturadorPorDefecto).FirstOrDefault(),
                    ObservacionComercial = x.Select(v => v.ObservacionComercial).FirstOrDefault()
                });

        // ── 3. Mapear y enriquecer DTOs ───────────────────────────────────────
        var result = new List<ClienteSelectorVentasDto>();

        foreach (var tercero in terceros)
        {
            var dto = new ClienteSelectorVentasDto
            {
                Id = tercero.Id,
                Legajo = tercero.Legajo,
                RazonSocial = tercero.RazonSocial,
                NombreFantasia = tercero.NombreFantasia,
                NroDocumento = tercero.NroDocumento,
                Facturable = tercero.Facturable,
                CondicionIvaId = tercero.CondicionIvaId,
                CondicionIvaDescripcion = condicionesIva.TryGetValue(tercero.CondicionIvaId, out var civa)
                    ? civa : string.Empty,
                MonedaId = tercero.MonedaId,
                LimiteCredito = tercero.LimiteCredito,
                PorcentajeMaximoDescuento = tercero.PorcentajeMaximoDescuento,
                Telefono = tercero.Telefono,
                Email = tercero.Email,
                VendedorId = tercero.VendedorId
            };

            if (perfilesPorTercero.TryGetValue(tercero.Id, out var perfilComercial))
            {
                dto.TienePerfilComercial = perfilComercial.TienePerfil;
                dto.RiesgoCrediticio = perfilComercial.RiesgoCrediticio;
                dto.SaldoMaximoVigente = perfilComercial.SaldoMaximoVigente;
                dto.CondicionCobranza = perfilComercial.CondicionCobranza;
                dto.CondicionVenta = perfilComercial.CondicionVenta;
                dto.PlazoCobro = perfilComercial.PlazoCobro;
                dto.FacturadorPorDefecto = perfilComercial.FacturadorPorDefecto;
                dto.ObservacionComercial = perfilComercial.ObservacionComercial;
            }

            if (tercero.MonedaId.HasValue && monedas.TryGetValue(tercero.MonedaId.Value, out var moneda))
                dto.MonedaDescripcion = moneda;

            if (tercero.VendedorId.HasValue && vendedores.TryGetValue(tercero.VendedorId.Value, out var vendedor))
                dto.VendedorNombre = vendedor;

            if (sucursalesEntregaPorTercero.TryGetValue(tercero.Id, out var sucursalEntrega))
            {
                dto.TieneSucursalesEntrega = sucursalEntrega.TieneSucursales;
                dto.SucursalEntregaPrincipalDescripcion = sucursalEntrega.PrincipalDescripcion;
            }

            dto.RequiereDefinirEntrega = RequiereDefinirEntrega(tercero.EsCliente, dto.TieneSucursalesEntrega);

            // Validación de si puede vender
            var (puedeVender, motivoBloqueo) = ValidarClienteParaVentas(
                tercero,
                estadosClientes,
                estadosPersonas);
            dto.PuedeVender = puedeVender;
            dto.MotivoBloqueo = motivoBloqueo;

            // Ubicación completa
            dto.UbicacionCompleta = BuildUbicacionCompleta(
                tercero,
                provincias,
                localidades,
                barrios);

            result.Add(dto);
        }

        return result;
    }

    private static (bool PuedeVender, string? MotivoBloqueo) ValidarClienteParaVentas(
        Domain.Entities.Terceros.Tercero tercero,
        Dictionary<long, (string Descripcion, bool Bloquea, bool Activo)> estadosClientes,
        Dictionary<long, (string Descripcion, bool Activo)> estadosPersonas)
    {
        // 1. Cliente debe estar activo
        if (!tercero.Activo)
            return (false, "Cliente inactivo");

        // 2. Validar estado de persona (si aplica)
        if (tercero.EstadoPersonaId.HasValue &&
            estadosPersonas.TryGetValue(tercero.EstadoPersonaId.Value, out var estadoPersona))
        {
            if (!estadoPersona.Activo)
                return (false, $"Estado de persona bloqueado: {estadoPersona.Descripcion}");
        }

        // 3. Validar estado de cliente (crítico para ventas)
        if (tercero.EstadoClienteId.HasValue &&
            estadosClientes.TryGetValue(tercero.EstadoClienteId.Value, out var estadoCliente))
        {
            if (!estadoCliente.Activo)
                return (false, $"Estado de cliente inactivo: {estadoCliente.Descripcion}");

            if (estadoCliente.Bloquea)
                return (false, $"Cliente bloqueado: {estadoCliente.Descripcion}");
        }

        // 4. Cliente debe ser facturable para ventas normales
        if (!tercero.Facturable)
            return (false, "Cliente no facturable");

        return (true, null);
    }

    private static string BuildUbicacionCompleta(
        Domain.Entities.Terceros.Tercero tercero,
        Dictionary<long, string> provincias,
        Dictionary<long, (string Descripcion, long ProvinciaId)> localidades,
        Dictionary<long, string> barrios)
    {
        var partes = new List<string>();

        var direccion = tercero.Domicilio.Completo;
        if (!string.IsNullOrWhiteSpace(direccion))
            partes.Add(direccion);

        string? barrioDesc = null;
        if (tercero.Domicilio.BarrioId.HasValue &&
            barrios.TryGetValue(tercero.Domicilio.BarrioId.Value, out var b))
            barrioDesc = b;

        string? localidadDesc = null;
        long? provinciaId = tercero.Domicilio.ProvinciaId;
        if (tercero.Domicilio.LocalidadId.HasValue &&
            localidades.TryGetValue(tercero.Domicilio.LocalidadId.Value, out var loc))
        {
            localidadDesc = loc.Descripcion;
            provinciaId ??= loc.ProvinciaId;
        }

        string? provinciaDesc = null;
        if (provinciaId.HasValue && provincias.TryGetValue(provinciaId.Value, out var prov))
            provinciaDesc = prov;

        var geografia = new List<string>();
        if (!string.IsNullOrWhiteSpace(barrioDesc))
            geografia.Add(barrioDesc);
        if (!string.IsNullOrWhiteSpace(localidadDesc))
            geografia.Add(localidadDesc);
        if (!string.IsNullOrWhiteSpace(provinciaDesc))
            geografia.Add(provinciaDesc);

        if (geografia.Count > 0)
            partes.Add(string.Join(" / ", geografia));

        if (!string.IsNullOrWhiteSpace(tercero.Domicilio.CodigoPostal))
            partes.Add($"CP {tercero.Domicilio.CodigoPostal}");

        return partes.Count > 0 ? string.Join(" - ", partes) : string.Empty;
    }

    private static bool RequiereDefinirEntrega(bool esCliente, bool tieneSucursalesEntrega)
        => esCliente && !tieneSucursalesEntrega;

    private static string NormalizeSearchTerm(string value)
        => value
            .Replace("-", string.Empty)
            .Replace(".", string.Empty)
            .Replace("/", string.Empty)
            .Replace(" ", string.Empty);
}
