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