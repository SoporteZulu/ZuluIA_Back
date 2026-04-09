using System.Text.Json;
using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;

namespace ZuluIA_Back.Application.Features.CRM.Commands;

public static class CrmDomainRules
{
    private static readonly StringComparer Comparer = StringComparer.OrdinalIgnoreCase;

    private static readonly HashSet<string> TiposCliente = new(Comparer) { "prospecto", "activo", "inactivo", "perdido" };
    private static readonly HashSet<string> SegmentosCliente = new(Comparer) { "pyme", "corporativo", "gobierno", "startup", "otro" };
    private static readonly HashSet<string> OrigenesCliente = new(Comparer) { "campana", "referido", "web", "llamada", "evento", "otro" };
    private static readonly HashSet<string> EstadosRelacion = new(Comparer) { "nuevo", "en_negociacion", "en_riesgo", "fidelizado" };
    private static readonly HashSet<string> CanalesContacto = new(Comparer) { "email", "telefono", "whatsapp", "presencial" };
    private static readonly HashSet<string> EstadosContacto = new(Comparer) { "activo", "no_contactar", "bounced", "inactivo" };
    private static readonly HashSet<string> EtapasOportunidad = new(Comparer) { "lead", "calificado", "propuesta", "negociacion", "cerrado_ganado", "cerrado_perdido" };
    private static readonly HashSet<string> Monedas = new(Comparer) { "USD", "ARS", "EUR", "MXN" };
    private static readonly HashSet<string> OrigenesOportunidad = new(Comparer) { "campana", "referido", "web", "llamada", "evento", "otro" };
    private static readonly HashSet<string> TiposInteraccion = new(Comparer) { "llamada", "email", "reunion", "visita", "ticket", "mensaje" };
    private static readonly HashSet<string> CanalesInteraccion = new(Comparer) { "telefono", "email", "whatsapp", "presencial", "videollamada" };
    private static readonly HashSet<string> ResultadosInteraccion = new(Comparer) { "exitosa", "sin_respuesta", "reprogramada", "cancelada" };
    private static readonly HashSet<string> TiposTarea = new(Comparer) { "llamar", "enviar_email", "preparar_propuesta", "visitar", "seguimiento", "otro" };
    private static readonly HashSet<string> PrioridadesTarea = new(Comparer) { "alta", "media", "baja" };
    private static readonly HashSet<string> EstadosTarea = new(Comparer) { "pendiente", "en_curso", "completada", "vencida" };
    private static readonly HashSet<string> TiposCampana = new(Comparer) { "email", "evento", "llamadas", "redes_sociales", "publicidad" };
    private static readonly HashSet<string> ObjetivosCampana = new(Comparer) { "generacion_leads", "upselling", "fidelizacion", "recuperacion", "branding" };
    private static readonly HashSet<string> TiposSegmento = new(Comparer) { "estatico", "dinamico" };
    private static readonly HashSet<string> RolesUsuario = new(Comparer) { "administrador", "supervisor", "comercial", "marketing", "soporte" };
    private static readonly HashSet<string> EstadosUsuario = new(Comparer) { "activo", "inactivo" };
    private static readonly HashSet<string> CamposSegmento = new(Comparer) { "nombre", "tipocliente", "segmento", "industria", "origencliente", "estadorelacion", "pais", "provincia", "ciudad" };
    private static readonly HashSet<string> OperadoresSegmento = new(Comparer) { "igual", "contiene", "mayor_que", "menor_que", "entre" };

    public static bool IsValidTipoCliente(string? value) => IsAllowed(value, TiposCliente);
    public static bool IsValidSegmentoCliente(string? value) => IsAllowed(value, SegmentosCliente);
    public static bool IsValidOrigenCliente(string? value) => IsAllowed(value, OrigenesCliente);
    public static bool IsValidEstadoRelacion(string? value) => IsAllowed(value, EstadosRelacion);
    public static bool IsValidCanalContacto(string? value) => IsAllowed(value, CanalesContacto);
    public static bool IsValidEstadoContacto(string? value) => IsAllowed(value, EstadosContacto);
    public static bool IsValidEtapaOportunidad(string? value) => IsAllowed(value, EtapasOportunidad);
    public static bool IsValidMoneda(string? value) => IsAllowed(value, Monedas);
    public static bool IsValidOrigenOportunidad(string? value) => IsAllowed(value, OrigenesOportunidad);
    public static bool IsValidTipoInteraccion(string? value) => IsAllowed(value, TiposInteraccion);
    public static bool IsValidCanalInteraccion(string? value) => IsAllowed(value, CanalesInteraccion);
    public static bool IsValidResultadoInteraccion(string? value) => IsAllowed(value, ResultadosInteraccion);
    public static bool IsValidTipoTarea(string? value) => IsAllowed(value, TiposTarea);
    public static bool IsValidPrioridadTarea(string? value) => IsAllowed(value, PrioridadesTarea);
    public static bool IsValidEstadoTarea(string? value) => IsAllowed(value, EstadosTarea);
    public static bool IsValidTipoCampana(string? value) => IsAllowed(value, TiposCampana);
    public static bool IsValidObjetivoCampana(string? value) => IsAllowed(value, ObjetivosCampana);
    public static bool IsValidTipoSegmento(string? value) => IsAllowed(value, TiposSegmento);
    public static bool IsValidRolUsuario(string? value) => IsAllowed(value, RolesUsuario);
    public static bool IsValidEstadoUsuario(string? value) => IsAllowed(value, EstadosUsuario);

    public static bool HasValidSegmentCriteriaJson(string? criteriosJson)
        => TryValidateSegmentCriteriaJson(criteriosJson, out _);

    public static bool HasValidSegmentDefinition(string? tipoSegmento, string? criteriosJson)
        => TryValidateSegmentDefinition(tipoSegmento, criteriosJson, out _);

    public static bool TryValidateSegmentDefinition(string? tipoSegmento, string? criteriosJson, out string? error)
    {
        error = null;

        if (!IsValidTipoSegmento(tipoSegmento))
        {
            error = "El tipo de segmento CRM no es válido.";
            return false;
        }

        if (!TryValidateSegmentCriteriaJson(criteriosJson, out error))
            return false;

        if (string.Equals(tipoSegmento?.Trim(), "estatico", StringComparison.OrdinalIgnoreCase)
            && !string.IsNullOrWhiteSpace(criteriosJson)
            && criteriosJson.Trim() != "[]")
        {
            error = "Los segmentos CRM estáticos no admiten criterios dinámicos.";
            return false;
        }

        return true;
    }

    public static bool TryValidateSegmentCriteriaJson(string? criteriosJson, out string? error)
    {
        error = null;
        if (string.IsNullOrWhiteSpace(criteriosJson))
            return true;

        try
        {
            using var document = JsonDocument.Parse(criteriosJson);
            if (document.RootElement.ValueKind != JsonValueKind.Array)
            {
                error = "Los criterios del segmento deben enviarse como un arreglo JSON.";
                return false;
            }

            foreach (var criterion in document.RootElement.EnumerateArray())
            {
                if (criterion.ValueKind != JsonValueKind.Object)
                {
                    error = "Cada criterio del segmento debe ser un objeto JSON válido.";
                    return false;
                }

                var campo = GetPropertyValue(criterion, "Campo") ?? GetPropertyValue(criterion, "campo");
                var operador = GetPropertyValue(criterion, "Operador") ?? GetPropertyValue(criterion, "operador");
                if (!IsAllowed(campo, CamposSegmento))
                {
                    error = "El segmento contiene un campo de criterio no soportado.";
                    return false;
                }

                if (!IsAllowed(operador, OperadoresSegmento))
                {
                    error = "El segmento contiene un operador de criterio no soportado.";
                    return false;
                }
            }

            return true;
        }
        catch (JsonException)
        {
            error = "Los criterios del segmento no tienen un JSON válido.";
            return false;
        }
    }

    private static bool IsAllowed(string? value, HashSet<string> allowed)
        => !string.IsNullOrWhiteSpace(value) && allowed.Contains(value.Trim());

    private static string? GetPropertyValue(JsonElement element, string propertyName)
        => element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String
            ? property.GetString()
            : null;
}

internal static class CrmWorkspaceReferenceValidation
{
    public static async Task<string?> ValidateClienteAsync(IApplicationDbContext db, long clienteId, CancellationToken ct)
    {
        var exists = await db.CrmClientes.AsNoTracking().AnyAsync(x => x.TerceroId == clienteId && x.Activo, ct);
        return exists ? null : $"Cliente CRM {clienteId} no encontrado o inactivo.";
    }

    public static async Task<string?> ValidateContactoAsync(IApplicationDbContext db, long clienteId, long? contactoId, CancellationToken ct)
    {
        if (!contactoId.HasValue)
            return null;

        var contacto = await db.CrmContactos.AsNoTracking().FirstOrDefaultAsync(x => x.Id == contactoId.Value && x.Activo, ct);
        if (contacto is null)
            return $"Contacto CRM {contactoId.Value} no encontrado o inactivo.";

        return contacto.ClienteId == clienteId
            ? null
            : $"El contacto CRM {contactoId.Value} no pertenece al cliente {clienteId}.";
    }

    public static async Task<string?> ValidateOportunidadAsync(IApplicationDbContext db, long? clienteId, long? oportunidadId, CancellationToken ct)
    {
        if (!oportunidadId.HasValue)
            return null;

        var oportunidad = await db.CrmOportunidades.AsNoTracking().FirstOrDefaultAsync(x => x.Id == oportunidadId.Value && x.Activa, ct);
        if (oportunidad is null)
            return $"Oportunidad CRM {oportunidadId.Value} no encontrada o inactiva.";

        return clienteId.HasValue && oportunidad.ClienteId != clienteId.Value
            ? $"La oportunidad CRM {oportunidadId.Value} no pertenece al cliente {clienteId.Value}."
            : null;
    }

    public static async Task<string?> ValidateUsuarioActivoAsync(IApplicationDbContext db, long? usuarioId, string descripcion, CancellationToken ct)
    {
        if (!usuarioId.HasValue)
            return null;

        var exists = await db.Usuarios.AsNoTracking().AnyAsync(x => x.Id == usuarioId.Value && x.Activo, ct);
        return exists ? null : $"El {descripcion} indicado no existe o está inactivo.";
    }

    public static async Task<string?> ValidateSegmentoAsync(IApplicationDbContext db, long? segmentoId, CancellationToken ct)
    {
        if (!segmentoId.HasValue)
            return null;

        var exists = await db.CrmSegmentos.AsNoTracking().AnyAsync(x => x.Id == segmentoId.Value && x.Activo, ct);
        return exists ? null : $"Segmento CRM {segmentoId.Value} no encontrado o inactivo.";
    }

    public static async Task<string?> ValidateSucursalAsync(IApplicationDbContext db, long sucursalId, CancellationToken ct)
    {
        var exists = await db.Sucursales.AsNoTracking().AnyAsync(x => x.Id == sucursalId && x.Activa, ct);
        return exists ? null : $"Sucursal {sucursalId} no encontrada o inactiva para CRM.";
    }

    public static async Task<string?> ValidateTipoRelacionContactoAsync(IApplicationDbContext db, long? tipoRelacionId, CancellationToken ct)
    {
        if (!tipoRelacionId.HasValue)
            return null;

        var exists = await db.TiposRelacionesContacto.AsNoTracking().AnyAsync(x => x.Id == tipoRelacionId.Value && x.Activo, ct);
        return exists ? null : $"El tipo de relación {tipoRelacionId.Value} no existe o está inactivo.";
    }
}

internal static class CrmLocalSchemaCompatibility
{
    private const string UndefinedTableSqlState = "42P01";
    private const string UndefinedColumnSqlState = "42703";

    public static bool TryTranslate(Exception exception, out string error)
    {
        error = string.Empty;

        if (TryGetDbException(exception) is not DbException dbException)
            return false;

        var sqlState = dbException.GetType().GetProperty("SqlState")?.GetValue(dbException) as string;
        var message = dbException.Message;

        if (Matches(sqlState, message, UndefinedTableSqlState, "CONTACTOS"))
        {
            error = "La base local no tiene la tabla CONTACTOS. Aplicá el script actualizado de `docs/crm-postgresql-local-script.md` para habilitar relaciones de contacto.";
            return true;
        }

        if (string.Equals(sqlState, UndefinedColumnSqlState, StringComparison.Ordinal)
            && message.Contains("terceros", StringComparison.OrdinalIgnoreCase))
        {
            error = "La base local no tiene todas las columnas requeridas en `terceros` para CRM. Aplicá el bloque de compatibilidad incluido en `docs/crm-postgresql-local-script.md`.";
            return true;
        }

        return false;
    }

    private static DbException? TryGetDbException(Exception exception)
        => exception switch
        {
            DbException dbException => dbException,
            DbUpdateException updateException when updateException.InnerException is DbException dbException => dbException,
            _ => null
        };

    private static bool Matches(string? sqlState, string message, string expectedSqlState, string identifier)
        => (string.Equals(sqlState, expectedSqlState, StringComparison.Ordinal)
            || message.Contains(expectedSqlState, StringComparison.Ordinal))
           && message.Contains(identifier, StringComparison.OrdinalIgnoreCase);
}
