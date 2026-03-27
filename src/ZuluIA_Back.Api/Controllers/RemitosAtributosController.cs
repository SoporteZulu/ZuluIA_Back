using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Comercial.DTOs;
using ZuluIA_Back.Application.Features.Reportes.DTOs;
using ZuluIA_Back.Application.Features.Reportes.Enums;
using ZuluIA_Back.Application.Features.Reportes.Services;
using ZuluIA_Back.Domain.Entities.Comercial;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Api.Controllers;

[ApiController]
public class RemitosAtributosController(
    IMediator mediator,
    IApplicationDbContext db,
    IRepository<ComprobanteItemAtributoComercial> repo,
    ICurrentUserService currentUser,
    ReporteExportacionService reporteExportacionService)
    : BaseController(mediator)
{
    [HttpGet("/api/remitos-atributos/comprobantes/{comprobanteId:long}")]
    public async Task<IActionResult> GetByComprobante(long comprobanteId, CancellationToken ct)
    {
        var items = await db.ComprobantesItemsAtributos.AsNoTracking()
            .Where(x => !x.IsDeleted)
            .Where(x => db.ComprobantesItems.Where(i => i.ComprobanteId == comprobanteId).Select(i => i.Id).Contains(x.ComprobanteItemId))
            .Join(db.AtributosComerciales.AsNoTracking(),
                x => x.AtributoComercialId,
                a => a.Id,
                (x, a) => new ComprobanteItemAtributoComercialDto
                {
                    Id = x.Id,
                    ComprobanteItemId = x.ComprobanteItemId,
                    AtributoComercialId = x.AtributoComercialId,
                    AtributoCodigo = a.Codigo,
                    AtributoDescripcion = a.Descripcion,
                    TipoDato = a.TipoDato.ToString().ToUpperInvariant(),
                    Valor = x.Valor,
                    CreatedAt = x.CreatedAt,
                    UpdatedAt = x.UpdatedAt
                })
            .OrderBy(x => x.ComprobanteItemId)
            .ThenBy(x => x.AtributoDescripcion)
            .ToListAsync(ct);

        return new OkObjectResult(items);
    }

    [HttpGet("/api/remitos-atributos/comprobante-items/{comprobanteItemId:long}")]
    public async Task<IActionResult> GetByComprobanteItem(long comprobanteItemId, CancellationToken ct)
    {
        var items = await db.ComprobantesItemsAtributos.AsNoTracking()
            .Where(x => x.ComprobanteItemId == comprobanteItemId && !x.IsDeleted)
            .Join(db.AtributosComerciales.AsNoTracking(),
                x => x.AtributoComercialId,
                a => a.Id,
                (x, a) => new ComprobanteItemAtributoComercialDto
                {
                    Id = x.Id,
                    ComprobanteItemId = x.ComprobanteItemId,
                    AtributoComercialId = x.AtributoComercialId,
                    AtributoCodigo = a.Codigo,
                    AtributoDescripcion = a.Descripcion,
                    TipoDato = a.TipoDato.ToString().ToUpperInvariant(),
                    Valor = x.Valor,
                    CreatedAt = x.CreatedAt,
                    UpdatedAt = x.UpdatedAt
                })
            .OrderBy(x => x.AtributoDescripcion)
            .ToListAsync(ct);

        return new OkObjectResult(items);
    }

    [HttpGet("/api/remitos-atributos/{id:long}")]
    public async Task<IActionResult> GetById(long id, CancellationToken ct)
    {
        var item = await db.ComprobantesItemsAtributos.AsNoTracking()
            .Where(x => x.Id == id && !x.IsDeleted)
            .Join(db.AtributosComerciales.AsNoTracking(),
                x => x.AtributoComercialId,
                a => a.Id,
                (x, a) => new ComprobanteItemAtributoComercialDto
                {
                    Id = x.Id,
                    ComprobanteItemId = x.ComprobanteItemId,
                    AtributoComercialId = x.AtributoComercialId,
                    AtributoCodigo = a.Codigo,
                    AtributoDescripcion = a.Descripcion,
                    TipoDato = a.TipoDato.ToString().ToUpperInvariant(),
                    Valor = x.Valor,
                    CreatedAt = x.CreatedAt,
                    UpdatedAt = x.UpdatedAt
                })
            .FirstOrDefaultAsync(ct);

        return item is null ? new NotFoundResult() : new OkObjectResult(item);
    }

    [HttpPost("/api/remitos-atributos/comprobante-items/{comprobanteItemId:long}")]
    public async Task<IActionResult> Upsert(long comprobanteItemId, [FromBody] RemitoAtributoRequest request, CancellationToken ct)
    {
        var item = await db.ComprobantesItems.AsNoTracking().FirstOrDefaultAsync(x => x.Id == comprobanteItemId, ct);
        if (item is null) return new NotFoundObjectResult(new { error = $"No se encontró el ítem de comprobante con ID {comprobanteItemId}." });

        var atributo = await db.AtributosComerciales.FirstOrDefaultAsync(x => x.Id == request.AtributoComercialId && !x.IsDeleted, ct);
        if (atributo is null || !atributo.Activo)
            return new NotFoundObjectResult(new { error = $"No se encontró el atributo comercial activo con ID {request.AtributoComercialId}." });

        try
        {
            atributo.ValidarValor(request.Valor);
        }
        catch (InvalidOperationException ex)
        {
            return new BadRequestObjectResult(new { error = ex.Message });
        }

        var existente = await repo.FirstOrDefaultAsync(x => x.ComprobanteItemId == comprobanteItemId && x.AtributoComercialId == request.AtributoComercialId, ct);
        if (existente is null)
        {
            var entity = ComprobanteItemAtributoComercial.Crear(comprobanteItemId, request.AtributoComercialId, request.Valor, currentUser.UserId);
            await repo.AddAsync(entity, ct);
            await db.SaveChangesAsync(ct);
            return new OkObjectResult(new { id = entity.Id });
        }

        existente.ActualizarValor(request.Valor, currentUser.UserId);
        repo.Update(existente);
        await db.SaveChangesAsync(ct);
        return new OkObjectResult(new { id = existente.Id });
    }

    [HttpDelete("/api/remitos-atributos/{id:long}")]
    public async Task<IActionResult> Delete(long id, CancellationToken ct)
    {
        var entity = await repo.GetByIdAsync(id, ct);
        if (entity is null) return new NotFoundResult();
        repo.Remove(entity);
        await db.SaveChangesAsync(ct);
        return new OkResult();
    }

    [HttpGet("/api/remitos-atributos/reportes/comprobante/{comprobanteId:long}")]
    public async Task<IActionResult> ExportarReporteComprobante(long comprobanteId, [FromQuery] FormatoExportacionReporte formato = FormatoExportacionReporte.Csv, CancellationToken ct = default)
    {
        var items = await db.ComprobantesItemsAtributos.AsNoTracking()
            .Where(x => !x.IsDeleted)
            .Where(x => db.ComprobantesItems.Where(i => i.ComprobanteId == comprobanteId).Select(i => i.Id).Contains(x.ComprobanteItemId))
            .Join(db.AtributosComerciales.AsNoTracking(),
                x => x.AtributoComercialId,
                a => a.Id,
                (x, a) => new { x.Id, x.ComprobanteItemId, a.Codigo, a.Descripcion, a.TipoDato, x.Valor })
            .OrderBy(x => x.ComprobanteItemId)
            .ThenBy(x => x.Descripcion)
            .ToListAsync(ct);

        var reporte = new ReporteTabularDto
        {
            Titulo = $"Atributos de comprobante {comprobanteId}",
            Columnas = ["Id", "ComprobanteItemId", "Codigo", "Descripcion", "TipoDato", "Valor"],
            Filas = items.Select(x => (IReadOnlyList<string>)new[]
            {
                x.Id.ToString(),
                x.ComprobanteItemId.ToString(),
                x.Codigo,
                x.Descripcion,
                x.TipoDato.ToString().ToUpperInvariant(),
                x.Valor
            }).ToList().AsReadOnly()
        };

        var archivo = reporteExportacionService.Exportar(reporte, formato, $"remitos_atributos_{comprobanteId}");
        return new FileContentResult(archivo.Contenido, archivo.ContentType) { FileDownloadName = archivo.NombreArchivo };
    }
}

public record RemitoAtributoRequest(long AtributoComercialId, string Valor);
