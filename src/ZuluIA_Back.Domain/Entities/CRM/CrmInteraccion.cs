using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.CRM;

public class CrmInteraccion : AuditableEntity
{
    public long ClienteId { get; private set; }
    public long? ContactoId { get; private set; }
    public long? OportunidadId { get; private set; }
    public string TipoInteraccion { get; private set; } = string.Empty;
    public string Canal { get; private set; } = string.Empty;
    public DateTimeOffset FechaHora { get; private set; }
    public long UsuarioResponsableId { get; private set; }
    public string Resultado { get; private set; } = string.Empty;
    public string? Descripcion { get; private set; }
    public string AdjuntosJson { get; private set; } = "[]";

    private CrmInteraccion() { }

    public static CrmInteraccion Crear(
        long clienteId,
        long? contactoId,
        long? oportunidadId,
        string tipoInteraccion,
        string canal,
        DateTimeOffset fechaHora,
        long usuarioResponsableId,
        string resultado,
        string? descripcion,
        string adjuntosJson,
        long? userId)
    {
        if (clienteId <= 0)
            throw new ArgumentException("La interacción CRM requiere un cliente válido.", nameof(clienteId));
        if (usuarioResponsableId <= 0)
            throw new ArgumentException("La interacción CRM requiere un responsable válido.", nameof(usuarioResponsableId));

        var entity = new CrmInteraccion
        {
            ClienteId = clienteId,
            ContactoId = contactoId,
            OportunidadId = oportunidadId,
            TipoInteraccion = NormalizeRequired(tipoInteraccion, nameof(tipoInteraccion)),
            Canal = NormalizeRequired(canal, nameof(canal)),
            FechaHora = fechaHora,
            UsuarioResponsableId = usuarioResponsableId,
            Resultado = NormalizeRequired(resultado, nameof(resultado)),
            Descripcion = NormalizeOptional(descripcion),
            AdjuntosJson = string.IsNullOrWhiteSpace(adjuntosJson) ? "[]" : adjuntosJson.Trim()
        };

        entity.SetCreated(userId);
        return entity;
    }

    public void Actualizar(
        long clienteId,
        long? contactoId,
        long? oportunidadId,
        string tipoInteraccion,
        string canal,
        DateTimeOffset fechaHora,
        long usuarioResponsableId,
        string resultado,
        string? descripcion,
        string adjuntosJson,
        long? userId)
    {
        if (clienteId <= 0)
            throw new ArgumentException("La interacción CRM requiere un cliente válido.", nameof(clienteId));
        if (usuarioResponsableId <= 0)
            throw new ArgumentException("La interacción CRM requiere un responsable válido.", nameof(usuarioResponsableId));

        ClienteId = clienteId;
        ContactoId = contactoId;
        OportunidadId = oportunidadId;
        TipoInteraccion = NormalizeRequired(tipoInteraccion, nameof(tipoInteraccion));
        Canal = NormalizeRequired(canal, nameof(canal));
        FechaHora = fechaHora;
        UsuarioResponsableId = usuarioResponsableId;
        Resultado = NormalizeRequired(resultado, nameof(resultado));
        Descripcion = NormalizeOptional(descripcion);
        AdjuntosJson = string.IsNullOrWhiteSpace(adjuntosJson) ? "[]" : adjuntosJson.Trim();
        SetUpdated(userId);
    }

    private static string NormalizeRequired(string value, string paramName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, paramName);
        return value.Trim();
    }

    private static string? NormalizeOptional(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
