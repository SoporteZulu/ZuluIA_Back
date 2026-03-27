using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.Produccion;

public class FormulaProduccionHistorial : AuditableEntity
{
    public long FormulaId { get; private set; }
    public int Version { get; private set; }
    public string Codigo { get; private set; } = string.Empty;
    public string Descripcion { get; private set; } = string.Empty;
    public decimal CantidadResultado { get; private set; }
    public string? Motivo { get; private set; }
    public string SnapshotJson { get; private set; } = string.Empty;

    private FormulaProduccionHistorial() { }

    public static FormulaProduccionHistorial Registrar(
        long formulaId,
        int version,
        string codigo,
        string descripcion,
        decimal cantidadResultado,
        string snapshotJson,
        string? motivo,
        long? userId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(codigo);
        ArgumentException.ThrowIfNullOrWhiteSpace(descripcion);
        ArgumentException.ThrowIfNullOrWhiteSpace(snapshotJson);

        var historial = new FormulaProduccionHistorial
        {
            FormulaId = formulaId,
            Version = version,
            Codigo = codigo.Trim(),
            Descripcion = descripcion.Trim(),
            CantidadResultado = cantidadResultado,
            SnapshotJson = snapshotJson,
            Motivo = motivo?.Trim()
        };

        historial.SetCreated(userId);
        return historial;
    }
}
