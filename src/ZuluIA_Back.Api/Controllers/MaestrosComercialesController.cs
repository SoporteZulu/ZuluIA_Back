using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Comercial.DTOs;
using ZuluIA_Back.Application.Features.Reportes.Enums;
using ZuluIA_Back.Application.Features.Reportes.Services;
using ZuluIA_Back.Application.Features.Reportes.DTOs;
using ZuluIA_Back.Domain.Entities.Comercial;
using ZuluIA_Back.Domain.Enums;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Api.Controllers;

[ApiController]
public class MaestrosComercialesController(
    IMediator mediator,
    IApplicationDbContext db,
    IRepository<MarcaComercial> marcasRepo,
    IRepository<ZonaComercial> zonasRepo,
    IRepository<JurisdiccionComercial> jurisdiccionesRepo,
    IRepository<MaestroAuxiliarComercial> auxiliaresRepo,
    IRepository<AtributoComercial> atributosRepo,
    ICurrentUserService currentUser,
    ReporteExportacionService reporteExportacionService)
    : BaseController(mediator)
{
    [HttpGet("/api/maestros-comerciales/marcas")]
    public async Task<IActionResult> GetMarcas([FromQuery] string? search = null, [FromQuery] bool? activo = null, CancellationToken ct = default)
        => new OkObjectResult((await AplicarFiltrosMarcas(db.MarcasComerciales.AsNoTracking(), search, activo).OrderBy(x => x.Descripcion).ToListAsync(ct)).Select(MapCatalogo));

    [HttpPost("/api/maestros-comerciales/marcas")]
    public async Task<IActionResult> CreateMarca([FromBody] CodigoDescripcionRequest request, CancellationToken ct)
    {
        if (await db.MarcasComerciales.AsNoTracking().AnyAsync(x => x.Codigo == request.Codigo.Trim().ToUpperInvariant(), ct))
            return new BadRequestObjectResult(new { error = $"Ya existe una marca comercial con código '{request.Codigo}'." });

        var entity = MarcaComercial.Crear(request.Codigo, request.Descripcion, currentUser.UserId);
        await marcasRepo.AddAsync(entity, ct);
        await db.SaveChangesAsync(ct);
        return new OkObjectResult(new { id = entity.Id });
    }

    [HttpGet("/api/maestros-comerciales/marcas/{id:long}")]
    public async Task<IActionResult> GetMarcaById(long id, CancellationToken ct)
        => WrapNotFound(await marcasRepo.GetByIdAsync(id, ct) is MarcaComercial entity ? MapCatalogo(entity) : null);

    [HttpPut("/api/maestros-comerciales/marcas/{id:long}")]
    public async Task<IActionResult> UpdateMarca(long id, [FromBody] DescripcionRequest request, CancellationToken ct)
    {
        var entity = await marcasRepo.GetByIdAsync(id, ct);
        if (entity is null) return new NotFoundResult();
        entity.Actualizar(request.Descripcion, currentUser.UserId);
        marcasRepo.Update(entity);
        await db.SaveChangesAsync(ct);
        return new OkResult();
    }

    [HttpDelete("/api/maestros-comerciales/marcas/{id:long}")]
    public async Task<IActionResult> DeleteMarca(long id, CancellationToken ct)
    {
        var entity = await marcasRepo.GetByIdAsync(id, ct);
        if (entity is null) return new NotFoundResult();
        entity.Desactivar(currentUser.UserId);
        marcasRepo.Update(entity);
        await db.SaveChangesAsync(ct);
        return new OkResult();
    }

    [HttpGet("/api/maestros-comerciales/zonas")]
    public async Task<IActionResult> GetZonas([FromQuery] string? search = null, [FromQuery] bool? activo = null, CancellationToken ct = default)
        => new OkObjectResult((await AplicarFiltrosZonas(db.ZonasComerciales.AsNoTracking(), search, activo).OrderBy(x => x.Descripcion).ToListAsync(ct)).Select(MapCatalogo));

    [HttpPost("/api/maestros-comerciales/zonas")]
    public async Task<IActionResult> CreateZona([FromBody] CodigoDescripcionRequest request, CancellationToken ct)
    {
        if (await db.ZonasComerciales.AsNoTracking().AnyAsync(x => x.Codigo == request.Codigo.Trim().ToUpperInvariant(), ct))
            return new BadRequestObjectResult(new { error = $"Ya existe una zona comercial con código '{request.Codigo}'." });

        var entity = ZonaComercial.Crear(request.Codigo, request.Descripcion, currentUser.UserId);
        await zonasRepo.AddAsync(entity, ct);
        await db.SaveChangesAsync(ct);
        return new OkObjectResult(new { id = entity.Id });
    }

    [HttpGet("/api/maestros-comerciales/zonas/{id:long}")]
    public async Task<IActionResult> GetZonaById(long id, CancellationToken ct)
        => WrapNotFound(await zonasRepo.GetByIdAsync(id, ct) is ZonaComercial entity ? MapCatalogo(entity) : null);

    [HttpPut("/api/maestros-comerciales/zonas/{id:long}")]
    public async Task<IActionResult> UpdateZona(long id, [FromBody] DescripcionRequest request, CancellationToken ct)
    {
        var entity = await zonasRepo.GetByIdAsync(id, ct);
        if (entity is null) return new NotFoundResult();
        entity.Actualizar(request.Descripcion, currentUser.UserId);
        zonasRepo.Update(entity);
        await db.SaveChangesAsync(ct);
        return new OkResult();
    }

    [HttpDelete("/api/maestros-comerciales/zonas/{id:long}")]
    public async Task<IActionResult> DeleteZona(long id, CancellationToken ct)
    {
        var entity = await zonasRepo.GetByIdAsync(id, ct);
        if (entity is null) return new NotFoundResult();
        entity.Desactivar(currentUser.UserId);
        zonasRepo.Update(entity);
        await db.SaveChangesAsync(ct);
        return new OkResult();
    }

    [HttpGet("/api/maestros-comerciales/jurisdicciones")]
    public async Task<IActionResult> GetJurisdicciones([FromQuery] string? search = null, [FromQuery] bool? activo = null, CancellationToken ct = default)
        => new OkObjectResult((await AplicarFiltrosJurisdicciones(db.JurisdiccionesComerciales.AsNoTracking(), search, activo).OrderBy(x => x.Descripcion).ToListAsync(ct)).Select(MapCatalogo));

    [HttpPost("/api/maestros-comerciales/jurisdicciones")]
    public async Task<IActionResult> CreateJurisdiccion([FromBody] CodigoDescripcionRequest request, CancellationToken ct)
    {
        if (await db.JurisdiccionesComerciales.AsNoTracking().AnyAsync(x => x.Codigo == request.Codigo.Trim().ToUpperInvariant(), ct))
            return new BadRequestObjectResult(new { error = $"Ya existe una jurisdicción comercial con código '{request.Codigo}'." });

        var entity = JurisdiccionComercial.Crear(request.Codigo, request.Descripcion, currentUser.UserId);
        await jurisdiccionesRepo.AddAsync(entity, ct);
        await db.SaveChangesAsync(ct);
        return new OkObjectResult(new { id = entity.Id });
    }

    [HttpGet("/api/maestros-comerciales/jurisdicciones/{id:long}")]
    public async Task<IActionResult> GetJurisdiccionById(long id, CancellationToken ct)
        => WrapNotFound(await jurisdiccionesRepo.GetByIdAsync(id, ct) is JurisdiccionComercial entity ? MapCatalogo(entity) : null);

    [HttpPut("/api/maestros-comerciales/jurisdicciones/{id:long}")]
    public async Task<IActionResult> UpdateJurisdiccion(long id, [FromBody] DescripcionRequest request, CancellationToken ct)
    {
        var entity = await jurisdiccionesRepo.GetByIdAsync(id, ct);
        if (entity is null) return new NotFoundResult();
        entity.Actualizar(request.Descripcion, currentUser.UserId);
        jurisdiccionesRepo.Update(entity);
        await db.SaveChangesAsync(ct);
        return new OkResult();
    }

    [HttpDelete("/api/maestros-comerciales/jurisdicciones/{id:long}")]
    public async Task<IActionResult> DeleteJurisdiccion(long id, CancellationToken ct)
    {
        var entity = await jurisdiccionesRepo.GetByIdAsync(id, ct);
        if (entity is null) return new NotFoundResult();
        entity.Desactivar(currentUser.UserId);
        jurisdiccionesRepo.Update(entity);
        await db.SaveChangesAsync(ct);
        return new OkResult();
    }

    [HttpGet("/api/maestros-comerciales/auxiliares")]
    public async Task<IActionResult> GetAuxiliares([FromQuery] string? grupo, [FromQuery] string? search = null, [FromQuery] bool? activo = null, CancellationToken ct = default)
    {
        var query = AplicarFiltrosAuxiliares(db.MaestrosAuxiliaresComerciales.AsNoTracking(), search, activo);
        if (!string.IsNullOrWhiteSpace(grupo))
            query = query.Where(x => x.Grupo == grupo.Trim().ToUpperInvariant());
        return new OkObjectResult((await query.OrderBy(x => x.Grupo).ThenBy(x => x.Descripcion).ToListAsync(ct)).Select(MapAuxiliar));
    }

    [HttpPost("/api/maestros-comerciales/auxiliares")]
    public async Task<IActionResult> CreateAuxiliar([FromBody] MaestroAuxiliarRequest request, CancellationToken ct)
    {
        var grupo = request.Grupo.Trim().ToUpperInvariant();
        var codigo = request.Codigo.Trim().ToUpperInvariant();
        if (await db.MaestrosAuxiliaresComerciales.AsNoTracking().AnyAsync(x => x.Grupo == grupo && x.Codigo == codigo, ct))
            return new BadRequestObjectResult(new { error = $"Ya existe un auxiliar comercial con grupo '{request.Grupo}' y código '{request.Codigo}'." });

        var entity = MaestroAuxiliarComercial.Crear(request.Grupo, request.Codigo, request.Descripcion, currentUser.UserId);
        await auxiliaresRepo.AddAsync(entity, ct);
        await db.SaveChangesAsync(ct);
        return new OkObjectResult(new { id = entity.Id });
    }

    [HttpGet("/api/maestros-comerciales/auxiliares/{id:long}")]
    public async Task<IActionResult> GetAuxiliarById(long id, CancellationToken ct)
        => WrapNotFound(await auxiliaresRepo.GetByIdAsync(id, ct) is MaestroAuxiliarComercial entity ? MapAuxiliar(entity) : null);

    [HttpPut("/api/maestros-comerciales/auxiliares/{id:long}")]
    public async Task<IActionResult> UpdateAuxiliar(long id, [FromBody] DescripcionRequest request, CancellationToken ct)
    {
        var entity = await auxiliaresRepo.GetByIdAsync(id, ct);
        if (entity is null) return new NotFoundResult();
        entity.Actualizar(request.Descripcion, currentUser.UserId);
        auxiliaresRepo.Update(entity);
        await db.SaveChangesAsync(ct);
        return new OkResult();
    }

    [HttpDelete("/api/maestros-comerciales/auxiliares/{id:long}")]
    public async Task<IActionResult> DeleteAuxiliar(long id, CancellationToken ct)
    {
        var entity = await auxiliaresRepo.GetByIdAsync(id, ct);
        if (entity is null) return new NotFoundResult();
        entity.Desactivar(currentUser.UserId);
        auxiliaresRepo.Update(entity);
        await db.SaveChangesAsync(ct);
        return new OkResult();
    }

    [HttpGet("/api/maestros-comerciales/atributos")]
    public async Task<IActionResult> GetAtributos([FromQuery] string? search = null, [FromQuery] bool? activo = null, [FromQuery] string? tipoDato = null, CancellationToken ct = default)
    {
        TipoDatoAtributoComercial? tipoDatoEnum = null;
        if (!string.IsNullOrWhiteSpace(tipoDato) && Enum.TryParse<TipoDatoAtributoComercial>(tipoDato, true, out var parsed))
            tipoDatoEnum = parsed;

        var query = AplicarFiltrosAtributos(db.AtributosComerciales.AsNoTracking(), search, activo);
        if (tipoDatoEnum.HasValue)
            query = query.Where(x => x.TipoDato == tipoDatoEnum.Value);

        return new OkObjectResult((await query.OrderBy(x => x.Descripcion).ToListAsync(ct)).Select(MapAtributo));
    }

    [HttpPost("/api/maestros-comerciales/atributos")]
    public async Task<IActionResult> CreateAtributo([FromBody] AtributoComercialRequest request, CancellationToken ct)
    {
        if (await db.AtributosComerciales.AsNoTracking().AnyAsync(x => x.Codigo == request.Codigo.Trim().ToUpperInvariant(), ct))
            return new BadRequestObjectResult(new { error = $"Ya existe un atributo comercial con código '{request.Codigo}'." });

        var entity = AtributoComercial.Crear(request.Codigo, request.Descripcion, request.TipoDato, currentUser.UserId);
        await atributosRepo.AddAsync(entity, ct);
        await db.SaveChangesAsync(ct);
        return new OkObjectResult(new { id = entity.Id });
    }

    [HttpGet("/api/maestros-comerciales/atributos/{id:long}")]
    public async Task<IActionResult> GetAtributoById(long id, CancellationToken ct)
        => WrapNotFound(await atributosRepo.GetByIdAsync(id, ct) is AtributoComercial entity ? MapAtributo(entity) : null);

    [HttpPut("/api/maestros-comerciales/atributos/{id:long}")]
    public async Task<IActionResult> UpdateAtributo(long id, [FromBody] AtributoComercialUpdateRequest request, CancellationToken ct)
    {
        var entity = await atributosRepo.GetByIdAsync(id, ct);
        if (entity is null) return new NotFoundResult();

        if (entity.TipoDato != request.TipoDato && await db.ComprobantesItemsAtributos.AsNoTracking().AnyAsync(x => x.AtributoComercialId == id && !x.IsDeleted, ct))
            return new BadRequestObjectResult(new { error = "No se puede cambiar el tipo de dato de un atributo comercial que ya posee valores asociados." });

        entity.Actualizar(request.Descripcion, request.TipoDato, currentUser.UserId);
        atributosRepo.Update(entity);
        await db.SaveChangesAsync(ct);
        return new OkResult();
    }

    [HttpDelete("/api/maestros-comerciales/atributos/{id:long}")]
    public async Task<IActionResult> DeleteAtributo(long id, CancellationToken ct)
    {
        var entity = await atributosRepo.GetByIdAsync(id, ct);
        if (entity is null) return new NotFoundResult();
        entity.Desactivar(currentUser.UserId);
        atributosRepo.Update(entity);
        await db.SaveChangesAsync(ct);
        return new OkResult();
    }

    [HttpGet("/api/maestros-comerciales/reportes/resumen")]
    public async Task<IActionResult> ExportarResumen([FromQuery] FormatoExportacionReporte formato = FormatoExportacionReporte.Csv, CancellationToken ct = default)
    {
        var reporte = new ReporteTabularDto
        {
            Titulo = "Resumen Maestros Comerciales",
            Columnas = ["Entidad", "Activos", "Inactivos"],
            Filas =
            [
                new[] { "Marcas", (await db.MarcasComerciales.AsNoTracking().CountAsync(x => x.Activo && !x.IsDeleted, ct)).ToString(), (await db.MarcasComerciales.AsNoTracking().CountAsync(x => !x.Activo || x.IsDeleted, ct)).ToString() },
                new[] { "Zonas", (await db.ZonasComerciales.AsNoTracking().CountAsync(x => x.Activo && !x.IsDeleted, ct)).ToString(), (await db.ZonasComerciales.AsNoTracking().CountAsync(x => !x.Activo || x.IsDeleted, ct)).ToString() },
                new[] { "Jurisdicciones", (await db.JurisdiccionesComerciales.AsNoTracking().CountAsync(x => x.Activo && !x.IsDeleted, ct)).ToString(), (await db.JurisdiccionesComerciales.AsNoTracking().CountAsync(x => !x.Activo || x.IsDeleted, ct)).ToString() },
                new[] { "Auxiliares", (await db.MaestrosAuxiliaresComerciales.AsNoTracking().CountAsync(x => x.Activo && !x.IsDeleted, ct)).ToString(), (await db.MaestrosAuxiliaresComerciales.AsNoTracking().CountAsync(x => !x.Activo || x.IsDeleted, ct)).ToString() },
                new[] { "Atributos", (await db.AtributosComerciales.AsNoTracking().CountAsync(x => x.Activo && !x.IsDeleted, ct)).ToString(), (await db.AtributosComerciales.AsNoTracking().CountAsync(x => !x.Activo || x.IsDeleted, ct)).ToString() }
            ]
        };

        var archivo = reporteExportacionService.Exportar(reporte, formato, "maestros_comerciales_resumen");
        return new FileContentResult(archivo.Contenido, archivo.ContentType) { FileDownloadName = archivo.NombreArchivo };
    }

    private static IQueryable<MarcaComercial> AplicarFiltrosMarcas(IQueryable<MarcaComercial> query, string? search, bool? activo)
    {
        query = query.Where(x => !x.IsDeleted);
        if (activo.HasValue)
            query = query.Where(x => x.Activo == activo.Value);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToUpperInvariant();
            query = query.Where(x => x.Codigo.Contains(term) || x.Descripcion.ToUpper().Contains(term));
        }

        return query;
    }

    private static IQueryable<ZonaComercial> AplicarFiltrosZonas(IQueryable<ZonaComercial> query, string? search, bool? activo)
    {
        query = query.Where(x => !x.IsDeleted);
        if (activo.HasValue)
            query = query.Where(x => x.Activo == activo.Value);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToUpperInvariant();
            query = query.Where(x => x.Codigo.Contains(term) || x.Descripcion.ToUpper().Contains(term));
        }

        return query;
    }

    private static IQueryable<JurisdiccionComercial> AplicarFiltrosJurisdicciones(IQueryable<JurisdiccionComercial> query, string? search, bool? activo)
    {
        query = query.Where(x => !x.IsDeleted);
        if (activo.HasValue)
            query = query.Where(x => x.Activo == activo.Value);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToUpperInvariant();
            query = query.Where(x => x.Codigo.Contains(term) || x.Descripcion.ToUpper().Contains(term));
        }

        return query;
    }

    private static IQueryable<MaestroAuxiliarComercial> AplicarFiltrosAuxiliares(IQueryable<MaestroAuxiliarComercial> query, string? search, bool? activo)
    {
        query = query.Where(x => !x.IsDeleted);
        if (activo.HasValue)
            query = query.Where(x => x.Activo == activo.Value);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToUpperInvariant();
            query = query.Where(x => x.Codigo.Contains(term) || x.Descripcion.ToUpper().Contains(term) || x.Grupo.Contains(term));
        }

        return query;
    }

    private static IQueryable<AtributoComercial> AplicarFiltrosAtributos(IQueryable<AtributoComercial> query, string? search, bool? activo)
    {
        query = query.Where(x => !x.IsDeleted);
        if (activo.HasValue)
            query = query.Where(x => x.Activo == activo.Value);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToUpperInvariant();
            query = query.Where(x => x.Codigo.Contains(term) || x.Descripcion.ToUpper().Contains(term));
        }

        return query;
    }

    private IActionResult WrapNotFound(object? entity)
        => entity is null ? new NotFoundResult() : new OkObjectResult(entity);

    private static CatalogoComercialDto MapCatalogo(MarcaComercial entity) => new()
    {
        Id = entity.Id,
        Codigo = entity.Codigo,
        Descripcion = entity.Descripcion,
        Activo = entity.Activo
    };

    private static CatalogoComercialDto MapCatalogo(ZonaComercial entity) => new()
    {
        Id = entity.Id,
        Codigo = entity.Codigo,
        Descripcion = entity.Descripcion,
        Activo = entity.Activo
    };

    private static CatalogoComercialDto MapCatalogo(JurisdiccionComercial entity) => new()
    {
        Id = entity.Id,
        Codigo = entity.Codigo,
        Descripcion = entity.Descripcion,
        Activo = entity.Activo
    };

    private static MaestroAuxiliarComercialDto MapAuxiliar(MaestroAuxiliarComercial entity) => new()
    {
        Id = entity.Id,
        Grupo = entity.Grupo,
        Codigo = entity.Codigo,
        Descripcion = entity.Descripcion,
        Activo = entity.Activo
    };

    private static AtributoComercialDto MapAtributo(AtributoComercial entity) => new()
    {
        Id = entity.Id,
        Codigo = entity.Codigo,
        Descripcion = entity.Descripcion,
        TipoDato = entity.TipoDato.ToString().ToUpperInvariant(),
        Activo = entity.Activo
    };
}

public record CodigoDescripcionRequest(string Codigo, string Descripcion);
public record DescripcionRequest(string Descripcion);
public record MaestroAuxiliarRequest(string Grupo, string Codigo, string Descripcion);
public record AtributoComercialRequest(string Codigo, string Descripcion, TipoDatoAtributoComercial TipoDato);
public record AtributoComercialUpdateRequest(string Descripcion, TipoDatoAtributoComercial TipoDato);
