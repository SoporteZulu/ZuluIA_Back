using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.CRM;

public class CrmTarea : AuditableEntity
{
    public long? ClienteId { get; private set; }
    public long? OportunidadId { get; private set; }
    public long AsignadoAId { get; private set; }
    public string Titulo { get; private set; } = string.Empty;
    public string? Descripcion { get; private set; }
    public string TipoTarea { get; private set; } = string.Empty;
    public DateOnly FechaVencimiento { get; private set; }
    public string Prioridad { get; private set; } = string.Empty;
    public string Estado { get; private set; } = string.Empty;
    public DateOnly? FechaCompletado { get; private set; }
    public bool Activa { get; private set; } = true;

    private CrmTarea() { }

    public static CrmTarea Crear(
        long? clienteId,
        long? oportunidadId,
        long asignadoAId,
        string titulo,
        string? descripcion,
        string tipoTarea,
        DateOnly fechaVencimiento,
        string prioridad,
        string estado,
        DateOnly? fechaCompletado,
        long? userId)
    {
        if (asignadoAId <= 0)
            throw new ArgumentException("La tarea CRM requiere un responsable válido.", nameof(asignadoAId));

        ValidateCompletion(estado, fechaCompletado, fechaVencimiento);
        var estadoNormalizado = NormalizeRequired(estado, nameof(estado));

        var entity = new CrmTarea
        {
            ClienteId = clienteId,
            OportunidadId = oportunidadId,
            AsignadoAId = asignadoAId,
            Titulo = NormalizeRequired(titulo, nameof(titulo)),
            Descripcion = NormalizeOptional(descripcion),
            TipoTarea = NormalizeRequired(tipoTarea, nameof(tipoTarea)),
            FechaVencimiento = fechaVencimiento,
            Prioridad = NormalizeRequired(prioridad, nameof(prioridad)),
            Estado = estadoNormalizado,
            FechaCompletado = NormalizeFechaCompletado(estadoNormalizado, fechaCompletado),
            Activa = true
        };

        entity.SetCreated(userId);
        return entity;
    }

    public void Actualizar(
        long? clienteId,
        long? oportunidadId,
        long asignadoAId,
        string titulo,
        string? descripcion,
        string tipoTarea,
        DateOnly fechaVencimiento,
        string prioridad,
        string estado,
        DateOnly? fechaCompletado,
        long? userId)
    {
        if (asignadoAId <= 0)
            throw new ArgumentException("La tarea CRM requiere un responsable válido.", nameof(asignadoAId));

        ValidateCompletion(estado, fechaCompletado, fechaVencimiento);
        var estadoNormalizado = NormalizeRequired(estado, nameof(estado));
        ValidateEstadoTransition(Estado, estadoNormalizado);

        ClienteId = clienteId;
        OportunidadId = oportunidadId;
        AsignadoAId = asignadoAId;
        Titulo = NormalizeRequired(titulo, nameof(titulo));
        Descripcion = NormalizeOptional(descripcion);
        TipoTarea = NormalizeRequired(tipoTarea, nameof(tipoTarea));
        FechaVencimiento = fechaVencimiento;
        Prioridad = NormalizeRequired(prioridad, nameof(prioridad));
        Estado = estadoNormalizado;
        FechaCompletado = NormalizeFechaCompletado(estadoNormalizado, fechaCompletado);
        Activa = true;
        SetUpdated(userId);
    }

    public void MarcarEliminada(long? userId)
    {
        Activa = false;
        SetDeleted();
        SetUpdated(userId);
    }

    /// <summary>
    /// Marca la tarea CRM como completada.
    /// </summary>
    public void Completar(DateOnly? fechaCompletado, long? userId)
    {
        var completionDate = fechaCompletado ?? DateOnly.FromDateTime(DateTime.UtcNow.Date);
        ValidateEstadoTransition(Estado, "completada");
        ValidateCompletion("completada", completionDate, FechaVencimiento);

        Estado = "completada";
        FechaCompletado = completionDate;
        SetUpdated(userId);
    }

    /// <summary>
    /// Reabre la tarea CRM dejándola nuevamente pendiente.
    /// </summary>
    public void Reabrir(long? userId)
    {
        ValidateEstadoTransition(Estado, "pendiente");

        Estado = "pendiente";
        FechaCompletado = null;
        SetUpdated(userId);
    }

    private static void ValidateCompletion(string estado, DateOnly? fechaCompletado, DateOnly fechaVencimiento)
    {
        if (string.Equals(estado?.Trim(), "completada", StringComparison.OrdinalIgnoreCase) && !fechaCompletado.HasValue)
            throw new ArgumentException("La tarea completada requiere fecha de completado.", nameof(fechaCompletado));

        if (fechaCompletado.HasValue && fechaCompletado.Value < fechaVencimiento.AddDays(-3650))
            throw new ArgumentException("La fecha de completado no es válida.", nameof(fechaCompletado));
    }

    private static DateOnly? NormalizeFechaCompletado(string estado, DateOnly? fechaCompletado)
        => string.Equals(estado, "completada", StringComparison.OrdinalIgnoreCase)
            ? fechaCompletado
            : null;

    private static void ValidateEstadoTransition(string currentState, string nextState)
    {
        if (string.IsNullOrWhiteSpace(currentState) || string.Equals(currentState, nextState, StringComparison.OrdinalIgnoreCase))
            return;

        var allowed = currentState.Trim().ToLowerInvariant() switch
        {
            "pendiente" => new[] { "en_curso", "completada", "vencida" },
            "en_curso" => new[] { "pendiente", "completada", "vencida" },
            "vencida" => new[] { "pendiente", "en_curso", "completada" },
            "completada" => new[] { "pendiente" },
            _ => []
        };

        if (!allowed.Contains(nextState.Trim(), StringComparer.OrdinalIgnoreCase))
            throw new InvalidOperationException($"La transición de estado '{currentState}' a '{nextState}' no está permitida para la tarea CRM.");
    }

    private static string NormalizeRequired(string value, string paramName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, paramName);
        return value.Trim();
    }

    private static string? NormalizeOptional(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
