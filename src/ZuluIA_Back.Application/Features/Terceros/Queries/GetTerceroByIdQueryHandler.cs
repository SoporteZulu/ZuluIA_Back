using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Features.Terceros.DTOs;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Interfaces;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Application.Features.Terceros.Queries;

public class GetTerceroByIdQueryHandler(
    ITerceroRepository repo,
    IApplicationDbContext db,
    IMapper mapper)
    : IRequestHandler<GetTerceroByIdQuery, Result<TerceroDto>>
{
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
            dto.CobradorNombre = cobrador?.NombreCompleto ?? string.Empty;
        }
        if (tercero.VendedorId.HasValue)
        {
            var vendedor = await db.Usuarios
                .FindAsync(new object[] { tercero.VendedorId.Value }, ct);
            dto.VendedorNombre = vendedor?.NombreCompleto ?? string.Empty;
        }

        // Sucursal (opcional)
        if (tercero.SucursalId.HasValue)
        {
            var sucursal = await db.Sucursales
                .FindAsync(new object[] { tercero.SucursalId.Value }, ct);
            dto.SucursalDescripcion = sucursal?.RazonSocial ?? string.Empty;
        }

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
        }
        else
        {
            dto.PerfilComercial = new TerceroPerfilComercialDto
            {
                TerceroId = tercero.Id,
                RiesgoCrediticio = RiesgoCrediticioComercial.Normal.ToString().ToUpperInvariant()
            };
        }

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
        if (tercero.Domicilio.LocalidadId.HasValue)
        {
            var localidad = await db.Localidades
                .FindAsync(new object[] { tercero.Domicilio.LocalidadId.Value }, ct);
            dto.Domicilio.LocalidadDescripcion = localidad?.Descripcion;
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

        return Result<TerceroDto>.Success(dto);
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