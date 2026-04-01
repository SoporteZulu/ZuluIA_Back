using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;

namespace ZuluIA_Back.Application.Features.Terceros.Services;

internal enum TipoOperacionTercero
{
    Cliente = 1,
    Proveedor = 2
}

public sealed class TerceroOperacionValidationService(IApplicationDbContext db)
{
    /// <summary>
    /// Valida que un tercero pueda operar como cliente en circuitos de venta.
    /// </summary>
    public Task<string?> ValidateClienteAsync(long terceroId, CancellationToken ct)
        => ValidateAsync(terceroId, TipoOperacionTercero.Cliente, ct);

    /// <summary>
    /// Valida que un tercero pueda operar como proveedor en circuitos de compra.
    /// </summary>
    public Task<string?> ValidateProveedorAsync(long terceroId, CancellationToken ct)
        => ValidateAsync(terceroId, TipoOperacionTercero.Proveedor, ct);

    private async Task<string?> ValidateAsync(long terceroId, TipoOperacionTercero tipoOperacion, CancellationToken ct)
    {
        var tercero = await db.Terceros
            .AsNoTracking()
            .Where(x => x.Id == terceroId)
            .Select(x => new
            {
                x.Id,
                x.Legajo,
                x.RazonSocial,
                x.Activo,
                x.DeletedAt,
                x.EsCliente,
                x.EsProveedor,
                x.EstadoPersonaId,
                x.EstadoClienteId,
                x.EstadoProveedorId
            })
            .FirstOrDefaultAsync(ct);

        if (tercero is null || tercero.DeletedAt.HasValue)
            return $"No se encontró el tercero con Id {terceroId}.";

        if (!tercero.Activo)
            return $"El tercero '{tercero.RazonSocial}' está inactivo.";

        if (tercero.EstadoPersonaId.HasValue)
        {
            var estadoGeneral = await db.EstadosPersonas
                .AsNoTracking()
                .Where(x => x.Id == tercero.EstadoPersonaId.Value && !x.IsDeleted)
                .Select(x => new { x.Descripcion, x.Activo })
                .FirstOrDefaultAsync(ct);

            if (estadoGeneral is null)
                return "El estado general del tercero no existe o fue eliminado.";

            if (!estadoGeneral.Activo)
                return $"El tercero '{tercero.RazonSocial}' tiene un estado general inactivo ({estadoGeneral.Descripcion}).";
        }

        if (tipoOperacion == TipoOperacionTercero.Cliente)
        {
            if (!tercero.EsCliente)
                return $"El tercero '{tercero.RazonSocial}' no tiene rol Cliente.";

            if (!tercero.EstadoClienteId.HasValue)
                return null;

            var estado = await db.EstadosClientes
                .AsNoTracking()
                .Where(x => x.Id == tercero.EstadoClienteId.Value && !x.IsDeleted)
                .Select(x => new { x.Descripcion, x.Activo, x.Bloquea })
                .FirstOrDefaultAsync(ct);

            if (estado is null)
                return "El estado comercial del cliente no existe o fue eliminado.";

            if (!estado.Activo)
                return $"El cliente '{tercero.RazonSocial}' tiene un estado inactivo ({estado.Descripcion}).";

            if (estado.Bloquea)
                return $"El cliente '{tercero.RazonSocial}' está bloqueado por su estado '{estado.Descripcion}'.";

            return null;
        }

        if (!tercero.EsProveedor)
            return $"El tercero '{tercero.RazonSocial}' no tiene rol Proveedor.";

        if (!tercero.EstadoProveedorId.HasValue)
            return null;

        var estadoProveedor = await db.EstadosProveedores
            .AsNoTracking()
            .Where(x => x.Id == tercero.EstadoProveedorId.Value && !x.IsDeleted)
            .Select(x => new { x.Descripcion, x.Activo, x.Bloquea })
            .FirstOrDefaultAsync(ct);

        if (estadoProveedor is null)
            return "El estado comercial del proveedor no existe o fue eliminado.";

        if (!estadoProveedor.Activo)
            return $"El proveedor '{tercero.RazonSocial}' tiene un estado inactivo ({estadoProveedor.Descripcion}).";

        if (estadoProveedor.Bloquea)
            return $"El proveedor '{tercero.RazonSocial}' está bloqueado por su estado '{estadoProveedor.Descripcion}'.";

        return null;
    }
}
