namespace ZuluIA_Back.Application.Features.Terceros.Queries;

internal static class TerceroEstadoOperativoResolver
{
    public static (string Codigo, string Descripcion, bool Bloquea) Resolve(
        bool activo,
        string? estadoPersonaDescripcion,
        bool? estadoPersonaActivo,
        bool esCliente,
        string? estadoClienteDescripcion,
        bool? estadoClienteActivo,
        bool? estadoClienteBloquea,
        bool esProveedor,
        string? estadoProveedorDescripcion,
        bool? estadoProveedorActivo,
        bool? estadoProveedorBloquea)
    {
        if (!activo)
            return ("INACTIVO", "Inactivo", true);

        if (estadoPersonaActivo.HasValue && !estadoPersonaActivo.Value)
            return ("ESTADO_GENERAL_INACTIVO", $"Estado general inactivo: {estadoPersonaDescripcion ?? "Sin descripción"}", true);

        if (esCliente && estadoClienteActivo.HasValue)
        {
            if (!estadoClienteActivo.Value)
                return ("ESTADO_CLIENTE_INACTIVO", $"Estado cliente inactivo: {estadoClienteDescripcion ?? "Sin descripción"}", true);

            if (estadoClienteBloquea == true)
                return ("CLIENTE_BLOQUEADO", estadoClienteDescripcion ?? "Cliente bloqueado", true);
        }

        if (esProveedor && estadoProveedorActivo.HasValue)
        {
            if (!estadoProveedorActivo.Value)
                return ("ESTADO_PROVEEDOR_INACTIVO", $"Estado proveedor inactivo: {estadoProveedorDescripcion ?? "Sin descripción"}", true);

            if (estadoProveedorBloquea == true)
                return ("PROVEEDOR_BLOQUEADO", estadoProveedorDescripcion ?? "Proveedor bloqueado", true);
        }

        return ("ACTIVO", "Activo", false);
    }
}
