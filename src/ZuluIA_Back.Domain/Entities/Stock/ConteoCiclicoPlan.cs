using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.Stock;

/// <summary>
/// Agenda operativa de conteos cíclicos para depósitos y zonas.
/// Cubre la planificación visible del módulo de conteos de Almacenes.
/// </summary>
public class ConteoCiclicoPlan : BaseEntity
{
    private static readonly HashSet<string> EstadosValidos =
    [
        "programado",
        "en-ejecucion",
        "observado"
    ];

    public string Deposito { get; private set; } = string.Empty;
    public string Zona { get; private set; } = string.Empty;
    public string Frecuencia { get; private set; } = string.Empty;
    public DateOnly ProximoConteo { get; private set; }
    public string Estado { get; private set; } = "programado";
    public decimal DivergenciaPct { get; private set; }
    public string Responsable { get; private set; } = string.Empty;
    public string Observacion { get; private set; } = string.Empty;
    public string NextStep { get; private set; } = string.Empty;
    public string ExecutionNote { get; private set; } = string.Empty;

    private ConteoCiclicoPlan() { }

    /// <summary>
    /// Crea un nuevo plan de conteo cíclico.
    /// </summary>
    public static ConteoCiclicoPlan Crear(
        string deposito,
        string zona,
        string frecuencia,
        DateOnly proximoConteo,
        string estado,
        decimal divergenciaPct,
        string? responsable,
        string? observacion,
        string? nextStep,
        string? executionNote)
    {
        Validar(deposito, zona, frecuencia, estado, divergenciaPct);

        return new ConteoCiclicoPlan
        {
            Deposito = deposito.Trim(),
            Zona = zona.Trim(),
            Frecuencia = frecuencia.Trim(),
            ProximoConteo = proximoConteo,
            Estado = NormalizarEstado(estado),
            DivergenciaPct = divergenciaPct,
            Responsable = NormalizarTexto(responsable, "Sin responsable"),
            Observacion = NormalizarTexto(observacion, "Sin observación adicional."),
            NextStep = NormalizarTexto(nextStep, "Sin próximo paso definido."),
            ExecutionNote = executionNote?.Trim() ?? string.Empty
        };
    }

    /// <summary>
    /// Actualiza los datos operativos del plan de conteo.
    /// </summary>
    public void Actualizar(
        string deposito,
        string zona,
        string frecuencia,
        DateOnly proximoConteo,
        string estado,
        decimal divergenciaPct,
        string? responsable,
        string? observacion,
        string? nextStep,
        string? executionNote)
    {
        Validar(deposito, zona, frecuencia, estado, divergenciaPct);

        Deposito = deposito.Trim();
        Zona = zona.Trim();
        Frecuencia = frecuencia.Trim();
        ProximoConteo = proximoConteo;
        Estado = NormalizarEstado(estado);
        DivergenciaPct = divergenciaPct;
        Responsable = NormalizarTexto(responsable, "Sin responsable");
        Observacion = NormalizarTexto(observacion, "Sin observación adicional.");
        NextStep = NormalizarTexto(nextStep, "Sin próximo paso definido.");
        ExecutionNote = executionNote?.Trim() ?? string.Empty;
    }

    private static void Validar(
        string deposito,
        string zona,
        string frecuencia,
        string estado,
        decimal divergenciaPct)
    {
        if (string.IsNullOrWhiteSpace(deposito))
            throw new ArgumentException("El depósito es obligatorio.");

        if (string.IsNullOrWhiteSpace(zona))
            throw new ArgumentException("La zona es obligatoria.");

        if (string.IsNullOrWhiteSpace(frecuencia))
            throw new ArgumentException("La frecuencia es obligatoria.");

        if (!EstadosValidos.Contains(NormalizarEstado(estado)))
            throw new ArgumentException("El estado del conteo no es válido.");

        if (divergenciaPct < 0)
            throw new ArgumentException("La divergencia no puede ser negativa.");
    }

    private static string NormalizarEstado(string estado)
        => estado.Trim().ToLowerInvariant();

    private static string NormalizarTexto(string? valor, string fallback)
        => string.IsNullOrWhiteSpace(valor)
            ? fallback
            : valor.Trim();
}
