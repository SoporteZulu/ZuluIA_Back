using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.CRM;

public class CrmOportunidad : AuditableEntity
{
    public long ClienteId { get; private set; }
    public long? ContactoPrincipalId { get; private set; }
    public string Titulo { get; private set; } = string.Empty;
    public string Etapa { get; private set; } = string.Empty;
    public int Probabilidad { get; private set; }
    public decimal MontoEstimado { get; private set; }
    public string Moneda { get; private set; } = string.Empty;
    public DateOnly FechaApertura { get; private set; }
    public DateOnly? FechaEstimadaCierre { get; private set; }
    public long? ResponsableId { get; private set; }
    public string Origen { get; private set; } = string.Empty;
    public string? MotivoPerdida { get; private set; }
    public string? Notas { get; private set; }
    public bool Activa { get; private set; } = true;

    private CrmOportunidad() { }

    public static CrmOportunidad Crear(
        long clienteId,
        long? contactoPrincipalId,
        string titulo,
        string etapa,
        int probabilidad,
        decimal montoEstimado,
        string moneda,
        DateOnly fechaApertura,
        DateOnly? fechaEstimadaCierre,
        long? responsableId,
        string origen,
        string? motivoPerdida,
        string? notas,
        long? userId)
    {
        if (clienteId <= 0)
            throw new ArgumentException("La oportunidad CRM requiere un cliente válido.", nameof(clienteId));

        ValidateProbabilidad(probabilidad);
        ValidateFechas(fechaApertura, fechaEstimadaCierre);
        var etapaNormalizada = NormalizeRequired(etapa, nameof(etapa));

        var entity = new CrmOportunidad
        {
            ClienteId = clienteId,
            ContactoPrincipalId = contactoPrincipalId,
            Titulo = NormalizeRequired(titulo, nameof(titulo)),
            Etapa = etapaNormalizada,
            Probabilidad = NormalizeProbabilidad(etapaNormalizada, probabilidad),
            MontoEstimado = montoEstimado,
            Moneda = NormalizeRequired(moneda, nameof(moneda)).ToUpperInvariant(),
            FechaApertura = fechaApertura,
            FechaEstimadaCierre = fechaEstimadaCierre,
            ResponsableId = responsableId,
            Origen = NormalizeRequired(origen, nameof(origen)),
            MotivoPerdida = NormalizeMotivoPerdida(etapaNormalizada, motivoPerdida),
            Notas = NormalizeOptional(notas),
            Activa = true
        };

        entity.SetCreated(userId);
        return entity;
    }

    public void Actualizar(
        long clienteId,
        long? contactoPrincipalId,
        string titulo,
        string etapa,
        int probabilidad,
        decimal montoEstimado,
        string moneda,
        DateOnly fechaApertura,
        DateOnly? fechaEstimadaCierre,
        long? responsableId,
        string origen,
        string? motivoPerdida,
        string? notas,
        long? userId)
    {
        if (clienteId <= 0)
            throw new ArgumentException("La oportunidad CRM requiere un cliente válido.", nameof(clienteId));

        ValidateProbabilidad(probabilidad);
        ValidateFechas(fechaApertura, fechaEstimadaCierre);
        var etapaNormalizada = NormalizeRequired(etapa, nameof(etapa));
        ValidateStageTransition(Etapa, etapaNormalizada);

        ClienteId = clienteId;
        ContactoPrincipalId = contactoPrincipalId;
        Titulo = NormalizeRequired(titulo, nameof(titulo));
        Etapa = etapaNormalizada;
        Probabilidad = NormalizeProbabilidad(etapaNormalizada, probabilidad);
        MontoEstimado = montoEstimado;
        Moneda = NormalizeRequired(moneda, nameof(moneda)).ToUpperInvariant();
        FechaApertura = fechaApertura;
        FechaEstimadaCierre = fechaEstimadaCierre;
        ResponsableId = responsableId;
        Origen = NormalizeRequired(origen, nameof(origen));
        MotivoPerdida = NormalizeMotivoPerdida(etapaNormalizada, motivoPerdida);
        Notas = NormalizeOptional(notas);
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
    /// Cierra la oportunidad CRM como ganada.
    /// </summary>
    public void CerrarGanada(long? userId)
    {
        if (IsClosedStage(Etapa))
            throw new InvalidOperationException("La oportunidad CRM ya se encuentra cerrada.");

        Etapa = "cerrado_ganado";
        Probabilidad = 100;
        MotivoPerdida = null;
        SetUpdated(userId);
    }

    /// <summary>
    /// Cierra la oportunidad CRM como perdida, registrando el motivo.
    /// </summary>
    public void CerrarPerdida(string motivoPerdida, long? userId)
    {
        if (IsClosedStage(Etapa))
            throw new InvalidOperationException("La oportunidad CRM ya se encuentra cerrada.");

        Etapa = "cerrado_perdido";
        Probabilidad = 0;
        MotivoPerdida = NormalizeRequired(motivoPerdida, nameof(motivoPerdida));
        SetUpdated(userId);
    }

    /// <summary>
    /// Reasigna el responsable comercial de la oportunidad CRM.
    /// </summary>
    public void ReasignarResponsable(long responsableId, long? userId)
    {
        if (responsableId <= 0)
            throw new ArgumentException("La oportunidad CRM requiere un responsable válido.", nameof(responsableId));
        if (IsClosedStage(Etapa))
            throw new InvalidOperationException("No se puede reasignar una oportunidad CRM ya cerrada.");

        ResponsableId = responsableId;
        SetUpdated(userId);
    }

    private static void ValidateProbabilidad(int probabilidad)
    {
        if (probabilidad is < 0 or > 100)
            throw new ArgumentException("La probabilidad debe estar entre 0 y 100.", nameof(probabilidad));
    }

    private static void ValidateFechas(DateOnly fechaApertura, DateOnly? fechaEstimadaCierre)
    {
        if (fechaEstimadaCierre.HasValue && fechaEstimadaCierre.Value < fechaApertura)
            throw new InvalidOperationException("La fecha estimada de cierre no puede ser anterior a la apertura.");
    }

    private static int NormalizeProbabilidad(string etapa, int probabilidad)
        => etapa.Trim().ToLowerInvariant() switch
        {
            "cerrado_ganado" => 100,
            "cerrado_perdido" => 0,
            _ => probabilidad
        };

    private static void ValidateStageTransition(string currentStage, string nextStage)
    {
        if (string.IsNullOrWhiteSpace(currentStage) || string.Equals(currentStage, nextStage, StringComparison.OrdinalIgnoreCase))
            return;

        if (IsClosedStage(currentStage))
            throw new InvalidOperationException("Una oportunidad CRM cerrada no puede volver a una etapa abierta ni cambiar su cierre.");
    }

    private static bool IsClosedStage(string stage)
        => string.Equals(stage?.Trim(), "cerrado_ganado", StringComparison.OrdinalIgnoreCase)
            || string.Equals(stage?.Trim(), "cerrado_perdido", StringComparison.OrdinalIgnoreCase);

    private static string? NormalizeMotivoPerdida(string etapa, string? motivoPerdida)
        => string.Equals(etapa?.Trim(), "cerrado_perdido", StringComparison.OrdinalIgnoreCase)
            ? NormalizeOptional(motivoPerdida)
            : null;

    private static string NormalizeRequired(string value, string paramName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, paramName);
        return value.Trim();
    }

    private static string? NormalizeOptional(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
