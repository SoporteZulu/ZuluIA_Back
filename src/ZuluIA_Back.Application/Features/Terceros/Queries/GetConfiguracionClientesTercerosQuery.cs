using MediatR;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Terceros.DTOs;

namespace ZuluIA_Back.Application.Features.Terceros.Queries;

/// <summary>
/// Devuelve en una sola respuesta la configuración base de la ficha de clientes de ventas.
/// </summary>
public record GetConfiguracionClientesTercerosQuery(bool SoloActivos = true)
    : IRequest<ConfiguracionClientesTercerosDto>;

public class GetConfiguracionClientesTercerosQueryHandler(
    IApplicationDbContext db,
    ISender sender)
    : IRequestHandler<GetConfiguracionClientesTercerosQuery, ConfiguracionClientesTercerosDto>
{
    public async Task<ConfiguracionClientesTercerosDto> Handle(GetConfiguracionClientesTercerosQuery request, CancellationToken ct)
    {
        var catalogos = await sender.Send(new GetCatalogosTercerosQuery(request.SoloActivos), ct);

        var categoriasTerceros = await db.CategoriasTerceros
            .AsNoTracking()
            .OrderBy(x => x.Descripcion)
            .Select(x => new ConfiguracionClienteReferenciaDto(x.Id, x.Descripcion))
            .ToListAsync(ct);

        var condicionesIva = await db.CondicionesIva
            .AsNoTracking()
            .OrderBy(x => x.Codigo)
            .Select(x => new ConfiguracionClienteCodigoDescripcionDto(x.Id, x.Codigo.ToString(), x.Descripcion))
            .ToListAsync(ct);

        var tiposDocumento = await db.TiposDocumento
            .AsNoTracking()
            .OrderBy(x => x.Codigo)
            .Select(x => new ConfiguracionClienteCodigoDescripcionDto(x.Id, x.Codigo.ToString(), x.Descripcion))
            .ToListAsync(ct);

        var monedasQuery = db.Monedas
            .AsNoTracking()
            .Where(x => x.DeletedAt == null);

        if (request.SoloActivos)
            monedasQuery = monedasQuery.Where(x => x.Activa);

        var monedas = await monedasQuery
            .OrderBy(x => x.Descripcion)
            .ThenBy(x => x.Codigo)
            .Select(x => new ConfiguracionClienteMonedaDto(x.Id, x.Codigo, x.Descripcion, x.Simbolo, x.SinDecimales))
            .ToListAsync(ct);

        var paises = await db.Paises
            .AsNoTracking()
            .Where(x => x.DeletedAt == null)
            .OrderBy(x => x.Descripcion)
            .ThenBy(x => x.Codigo)
            .Select(x => new ConfiguracionClienteReferenciaDto(x.Id, x.Descripcion))
            .ToListAsync(ct);

        var zonasQuery = db.ZonasComerciales
            .AsNoTracking()
            .Where(x => x.DeletedAt == null);

        if (request.SoloActivos)
            zonasQuery = zonasQuery.Where(x => x.Activo);

        var zonasComerciales = await zonasQuery
            .OrderBy(x => x.Descripcion)
            .ThenBy(x => x.Codigo)
            .Select(x => new ConfiguracionClienteCodigoDescripcionDto(x.Id, x.Codigo, x.Descripcion))
            .ToListAsync(ct);

        var sucursalesQuery = db.Sucursales
            .AsNoTracking()
            .Where(x => x.DeletedAt == null);

        if (request.SoloActivos)
            sucursalesQuery = sucursalesQuery.Where(x => x.Activa);

        var sucursales = await sucursalesQuery
            .OrderByDescending(x => x.CasaMatriz)
            .ThenBy(x => x.RazonSocial)
            .Select(x => new ConfiguracionClienteSucursalDto(x.Id, x.RazonSocial, x.NombreFantasia, x.CasaMatriz))
            .ToListAsync(ct);

        var usuariosQuery = db.Usuarios
            .AsNoTracking()
            .Where(x => x.DeletedAt == null);

        if (request.SoloActivos)
            usuariosQuery = usuariosQuery.Where(x => x.Activo);

        var usuariosComerciales = await usuariosQuery
            .OrderBy(x => x.NombreCompleto ?? x.UserName)
            .ThenBy(x => x.UserName)
            .Select(x => new ConfiguracionClienteUsuarioComercialDto(x.Id, x.UserName, x.NombreCompleto, x.Email))
            .ToListAsync(ct);

        return new ConfiguracionClientesTercerosDto(
            catalogos,
            categoriasTerceros,
            condicionesIva,
            tiposDocumento,
            monedas,
            paises,
            zonasComerciales,
            sucursales,
            usuariosComerciales,
            usuariosComerciales);
    }
}
