using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Configuracion.Commands;
using ZuluIA_Back.Application.Features.Configuracion.Queries;
using ZuluIA_Back.Infrastructure.Common.Interfaces;

namespace ZuluIA_Back.Api.Controllers;

public class ConfiguracionController(IMediator mediator, IApplicationDbContext db)
    : BaseController(mediator)
{
    // ─────────────────────────────────────────────────────────────────────────
    // PARÁMETROS DEL SISTEMA (tabla config)
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Retorna todos los parámetros de configuración del sistema.
    /// </summary>
    [HttpGet("parametros")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetParametros(CancellationToken ct)
    {
        var result = await Mediator.Send(new GetConfiguracionQuery(), ct);
        return Ok(result);
    }

    /// <summary>
    /// Crea o actualiza un parámetro de configuración del sistema (upsert por campo).
    /// </summary>
    [HttpPost("parametros")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SetParametro(
        [FromBody] SetConfiguracionCommand command,
        CancellationToken ct)
    {
        var result = await Mediator.Send(command, ct);
        return FromResult(result);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // TABLAS DE REFERENCIA (read-only, sin CQRS — son lookup tables)
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Retorna las monedas activas del sistema.
    /// NOTA: Antes devolvía Países por bug — ahora lee la tabla monedas correctamente.
    /// </summary>
    [HttpGet("monedas")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMonedas(CancellationToken ct)
    {
        // ✅ FIX: antes usaba db.Paises — ahora usa db.Monedas correctamente
        var monedas = await db.Monedas
            .AsNoTracking()
            .Where(x => x.Activa)
            .OrderBy(x => x.Descripcion)
            .Select(x => new
            {
                x.Id,
                x.Codigo,
                x.Descripcion,
                x.Simbolo,
                x.SinDecimales
            })
            .ToListAsync(ct);

        return Ok(monedas);
    }

    /// <summary>
    /// Retorna las condiciones IVA disponibles (desde la tabla condiciones_iva).
    /// </summary>
    [HttpGet("condiciones-iva")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCondicionesIva(CancellationToken ct)
    {
        // ✅ MEJORADO: lee desde DB en lugar de hardcoded
        var condiciones = await db.CondicionesIva
            .AsNoTracking()
            .OrderBy(x => x.Codigo)
            .Select(x => new { x.Id, x.Codigo, x.Descripcion })
            .ToListAsync(ct);

        return Ok(condiciones);
    }

    /// <summary>
    /// Retorna los tipos de documento disponibles (desde la tabla tipos_documento).
    /// </summary>
    [HttpGet("tipos-documento")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTiposDocumento(CancellationToken ct)
    {
        // ✅ MEJORADO: lee desde DB en lugar de hardcoded
        var tipos = await db.TiposDocumento
            .AsNoTracking()
            .OrderBy(x => x.Codigo)
            .Select(x => new { x.Id, x.Codigo, x.Descripcion })
            .ToListAsync(ct);

        return Ok(tipos);
    }

    /// <summary>
    /// Retorna los tipos de comprobante activos.
    /// </summary>
    [HttpGet("tipos-comprobante")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTiposComprobante(CancellationToken ct)
    {
        var tipos = await db.TiposComprobante
            .AsNoTracking()
            .Where(x => x.Activo)
            .OrderBy(x => x.Descripcion)
            .Select(x => new
            {
                x.Id,
                x.Codigo,
                x.Descripcion,
                x.EsVenta,
                x.EsCompra,
                x.EsInterno,
                x.AfectaStock,
                x.AfectaCuentaCorriente,
                x.GeneraAsiento,
                x.TipoAfip,
                x.LetraAfip
            })
            .ToListAsync(ct);

        return Ok(tipos);
    }

    /// <summary>
    /// Retorna las alícuotas de IVA disponibles.
    /// </summary>
    [HttpGet("alicuotas-iva")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAlicuotasIva(CancellationToken ct)
    {
        var alicuotas = await db.AlicuotasIva
            .AsNoTracking()
            .OrderBy(x => x.Porcentaje)
            .Select(x => new { x.Id, x.Codigo, x.Descripcion, x.Porcentaje })
            .ToListAsync(ct);

        return Ok(alicuotas);
    }

    /// <summary>
    /// Retorna las unidades de medida.
    /// </summary>
    [HttpGet("unidades-medida")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUnidadesMedida(CancellationToken ct)
    {
        var unidades = await db.UnidadesMedida
            .AsNoTracking()
            .OrderBy(x => x.Descripcion)
            .Select(x => new { x.Id, x.Codigo, x.Descripcion })
            .ToListAsync(ct);

        return Ok(unidades);
    }

    /// <summary>
    /// Retorna las formas de pago activas.
    /// </summary>
    [HttpGet("formas-pago")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetFormasPago(CancellationToken ct)
    {
        var formas = await db.FormasPago
            .AsNoTracking()
            .Where(x => x.Activa)
            .OrderBy(x => x.Descripcion)
            .Select(x => new { x.Id, x.Descripcion })
            .ToListAsync(ct);

        return Ok(formas);
    }

    /// <summary>
    /// Retorna las categorías de terceros (clientes/proveedores).
    /// </summary>
    [HttpGet("categorias-terceros")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCategoriasTerceros(CancellationToken ct)
    {
        var categorias = await db.CategoriasTerceros
            .AsNoTracking()
            .OrderBy(x => x.Descripcion)
            .Select(x => new { x.Id, x.Descripcion, x.EsImportador })
            .ToListAsync(ct);

        return Ok(categorias);
    }
}