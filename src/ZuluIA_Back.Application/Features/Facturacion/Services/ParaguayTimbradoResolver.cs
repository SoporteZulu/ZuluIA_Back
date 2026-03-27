using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Facturacion.DTOs;

namespace ZuluIA_Back.Application.Features.Facturacion.Services;

public static class ParaguayTimbradoResolver
{
    public static async Task<ValidacionTimbradoParaguayDto> ValidarAsync(
        IApplicationDbContext db,
        long sucursalId,
        long? puntoFacturacionId,
        long tipoComprobanteId,
        DateOnly fecha,
        long numeroComprobante,
        CancellationToken ct)
    {
        var result = new ValidacionTimbradoParaguayDto
        {
            SucursalId = sucursalId,
            PuntoFacturacionId = puntoFacturacionId,
            TipoComprobanteId = tipoComprobanteId,
            Fecha = fecha,
            NumeroComprobante = numeroComprobante,
            EsValido = true
        };

        var sucursal = await db.Sucursales
            .AsNoTracking()
            .Where(x => x.Id == sucursalId)
            .Select(x => new { x.PaisId })
            .FirstOrDefaultAsync(ct);

        if (sucursal is null)
            return result;

        var paisCodigo = await db.Paises
            .AsNoTracking()
            .Where(x => x.Id == sucursal.PaisId)
            .Select(x => x.Codigo)
            .FirstOrDefaultAsync(ct);

        result.EsSucursalParaguay = EsCodigoParaguay(paisCodigo);
        result.RequiereTimbrado = result.EsSucursalParaguay;

        if (!result.EsSucursalParaguay)
            return result;

        if (!puntoFacturacionId.HasValue)
        {
            result.EsValido = false;
            result.Mensaje = "La emisión de comprobantes para sucursales Paraguay requiere punto de facturación para validar timbrado.";
            return result;
        }

        if (numeroComprobante > int.MaxValue)
        {
            result.EsValido = false;
            result.Mensaje = "El número de comprobante excede el rango soportado por timbrado Paraguay.";
            return result;
        }

        var numero = (int)numeroComprobante;
        var timbrado = await db.Timbrados
            .AsNoTracking()
            .Where(x =>
                x.Activo &&
                x.SucursalId == sucursalId &&
                x.PuntoFacturacionId == puntoFacturacionId.Value &&
                x.TipoComprobanteId == tipoComprobanteId &&
                x.FechaInicio <= fecha &&
                x.FechaFin >= fecha &&
                x.NroComprobanteDesde <= numero &&
                x.NroComprobanteHasta >= numero)
            .OrderBy(x => x.FechaInicio)
            .ThenBy(x => x.Id)
            .Select(x => new
            {
                x.Id,
                x.NroTimbrado,
                x.FechaInicio,
                x.FechaFin,
                x.NroComprobanteDesde,
                x.NroComprobanteHasta
            })
            .FirstOrDefaultAsync(ct);

        if (timbrado is null)
        {
            result.EsValido = false;
            result.Mensaje = "No existe un timbrado vigente para la sucursal Paraguay, el punto de facturación, el tipo y el número de comprobante indicado.";
            return result;
        }

        result.TimbradoId = timbrado.Id;
        result.NroTimbrado = timbrado.NroTimbrado;
        result.FechaInicio = timbrado.FechaInicio;
        result.FechaFin = timbrado.FechaFin;
        result.NroComprobanteDesde = timbrado.NroComprobanteDesde;
        result.NroComprobanteHasta = timbrado.NroComprobanteHasta;
        result.Mensaje = "Timbrado vigente encontrado.";
        return result;
    }

    private static bool EsCodigoParaguay(string? codigo)
    {
        if (string.IsNullOrWhiteSpace(codigo))
            return false;

        var normalized = codigo.Trim().ToUpperInvariant();
        return normalized is "PY" or "PRY";
    }
}