using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.Contabilidad;

/// <summary>
/// Vincula una cuenta contable con un registro de otra tabla
/// (ej: asociar la cuenta de IVA Ventas al tipo de comprobante).
/// </summary>
public class PlanCuentaParametro : BaseEntity
{
    public long EjercicioId { get; private set; }
    public long CuentaId { get; private set; }
    public string Tabla { get; private set; } = string.Empty;
    public long IdRegistro { get; private set; }

    private PlanCuentaParametro() { }

    public static PlanCuentaParametro Crear(
        long ejercicioId,
        long cuentaId,
        string tabla,
        long idRegistro)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tabla);
        return new PlanCuentaParametro
        {
            EjercicioId = ejercicioId,
            CuentaId    = cuentaId,
            Tabla       = tabla.Trim().ToLowerInvariant(),
            IdRegistro  = idRegistro
        };
    }
}