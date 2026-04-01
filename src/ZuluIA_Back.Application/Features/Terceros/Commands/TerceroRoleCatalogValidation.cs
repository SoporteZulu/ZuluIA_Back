using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;

namespace ZuluIA_Back.Application.Features.Terceros.Commands;

internal static class TerceroRoleCatalogValidation
{
    public static async Task<string?> ValidateAsync(
        IApplicationDbContext db,
        bool esCliente,
        bool esProveedor,
        long? categoriaClienteId,
        long? estadoClienteId,
        long? categoriaProveedorId,
        long? estadoProveedorId,
        CancellationToken ct)
    {
        if (categoriaClienteId.HasValue)
        {
            if (!esCliente)
                return "No puede informar categoría de cliente si el tercero no tiene rol Cliente.";

            var exists = await db.CategoriasClientes
                .AsNoTracking()
                .AnyAsync(x => x.Id == categoriaClienteId.Value && !x.IsDeleted, ct);

            if (!exists)
                return "La categoría de cliente indicada no existe.";
        }

        if (estadoClienteId.HasValue)
        {
            if (!esCliente)
                return "No puede informar estado de cliente si el tercero no tiene rol Cliente.";

            var exists = await db.EstadosClientes
                .AsNoTracking()
                .AnyAsync(x => x.Id == estadoClienteId.Value && !x.IsDeleted, ct);

            if (!exists)
                return "El estado de cliente indicado no existe.";
        }

        if (categoriaProveedorId.HasValue)
        {
            if (!esProveedor)
                return "No puede informar categoría de proveedor si el tercero no tiene rol Proveedor.";

            var exists = await db.CategoriasProveedores
                .AsNoTracking()
                .AnyAsync(x => x.Id == categoriaProveedorId.Value && !x.IsDeleted, ct);

            if (!exists)
                return "La categoría de proveedor indicada no existe.";
        }

        if (estadoProveedorId.HasValue)
        {
            if (!esProveedor)
                return "No puede informar estado de proveedor si el tercero no tiene rol Proveedor.";

            var exists = await db.EstadosProveedores
                .AsNoTracking()
                .AnyAsync(x => x.Id == estadoProveedorId.Value && !x.IsDeleted, ct);

            if (!exists)
                return "El estado de proveedor indicado no existe.";
        }

        return null;
    }
}
