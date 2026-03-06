using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.Contabilidad;

public class PlanCuenta : BaseEntity
{
    public long EjercicioId { get; private set; }
    public long? IntegradoraId { get; private set; }
    public string CodigoCuenta { get; private set; } = string.Empty;
    public string Denominacion { get; private set; } = string.Empty;
    public short Nivel { get; private set; }
    public string OrdenNivel { get; private set; } = string.Empty;
    public bool Imputable { get; private set; } = true;
    public string? Tipo { get; private set; }
    public char? SaldoNormal { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    private readonly List<PlanCuenta> _subcuentas = [];
    public IReadOnlyCollection<PlanCuenta> Subcuentas => _subcuentas.AsReadOnly();

    private PlanCuenta() { }

    public static PlanCuenta Crear(
        long ejercicioId,
        long? integradoraId,
        string codigoCuenta,
        string denominacion,
        short nivel,
        string ordenNivel,
        bool imputable,
        string? tipo,
        char? saldoNormal)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(codigoCuenta);
        ArgumentException.ThrowIfNullOrWhiteSpace(denominacion);

        return new PlanCuenta
        {
            EjercicioId   = ejercicioId,
            IntegradoraId = integradoraId,
            CodigoCuenta  = codigoCuenta.Trim(),
            Denominacion  = denominacion.Trim(),
            Nivel         = nivel,
            OrdenNivel    = ordenNivel.Trim(),
            Imputable     = imputable,
            Tipo          = tipo?.Trim().ToUpperInvariant(),
            SaldoNormal   = saldoNormal,
            CreatedAt     = DateTimeOffset.UtcNow,
            UpdatedAt     = DateTimeOffset.UtcNow
        };
    }

    public void Actualizar(
        string denominacion,
        bool imputable,
        string? tipo,
        char? saldoNormal)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(denominacion);
        Denominacion = denominacion.Trim();
        Imputable    = imputable;
        Tipo         = tipo?.Trim().ToUpperInvariant();
        SaldoNormal  = saldoNormal;
        UpdatedAt    = DateTimeOffset.UtcNow;
    }

    public bool EsDeudora => SaldoNormal == 'D' || SaldoNormal == 'd';
    public bool EsAcreedora => SaldoNormal == 'A' || SaldoNormal == 'a';
}