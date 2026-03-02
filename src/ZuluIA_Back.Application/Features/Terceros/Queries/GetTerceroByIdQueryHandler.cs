using AutoMapper;
using MediatR;
using ZuluIA_Back.Application.Features.Terceros.DTOs;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Interfaces;

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
            return (Result<TerceroDto>)Result<TerceroDto>.Failure(
                $"No se encontró el tercero con Id {request.Id}.");

        // ── 2. Mapeo base (campos escalares + Domicilio VO) ───────────────────
        var dto = mapper.Map<TerceroDto>(tercero);

        // ── 3. Resolver descripciones de catálogos ────────────────────────────
        // Usa los métodos RAW para obtener los datos como string.
        // Cuando tengas los DbSet reales, reemplaza estos bloques por búsquedas a los DbSet.

        // TipoDocumento
        var tipoDocList = await db.GetTipoDocumentosRawAsync(ct); // Devuelve lista de string (solo los nombres, ejemplo)
        dto.TipoDocumentoDescripcion = tipoDocList.FirstOrDefault() ?? string.Empty;

        // CondiciónIVA
        var condIvaList = await db.GetCondicionesIvaRawAsync(ct);
        dto.CondicionIvaDescripcion = condIvaList.FirstOrDefault() ?? string.Empty;

        // Categoría (opcional)
        var catList = await db.GetCategoriasTerceroRawAsync(ct);
        dto.CategoriaDescripcion = catList.FirstOrDefault() ?? string.Empty;

        // Moneda (opcional)
        var monedaList = await db.GetMonedasRawAsync(ct);
        dto.MonedaDescripcion = monedaList.FirstOrDefault() ?? string.Empty;

        // Cobrador/Vendedor (opcional)
        var usuariosList = await db.GetUsuariosRawAsync(ct);
        dto.CobradorNombre = usuariosList.FirstOrDefault() ?? string.Empty;
        dto.VendedorNombre = usuariosList.FirstOrDefault() ?? string.Empty;

        // Sucursal (opcional)
        var sucursalList = await db.GetSucursalesRawAsync(ct);
        dto.SucursalDescripcion = sucursalList.FirstOrDefault() ?? string.Empty;

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