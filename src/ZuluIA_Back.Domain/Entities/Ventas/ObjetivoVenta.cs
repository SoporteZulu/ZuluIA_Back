using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.Ventas;

public class ObjetivoVenta : AuditableEntity
{
    public long SucursalId { get; private set; }
    public long VendedorId { get; private set; }
    /// <summary>Período en formato YYYYMM.</summary>
    public int Periodo { get; private set; }
    public decimal MontoObjetivo { get; private set; }
    public decimal MontoRealizado { get; private set; }
    public string? Descripcion { get; private set; }
    public bool Cerrado { get; private set; }

    private ObjetivoVenta() { }

    public static ObjetivoVenta Crear(
        long sucursalId,
        long vendedorId,
        int periodo,
        decimal montoObjetivo,
        string? descripcion,
        long? userId)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(montoObjetivo);
        var objetivo = new ObjetivoVenta
        {
            SucursalId     = sucursalId,
            VendedorId     = vendedorId,
            Periodo        = periodo,
            MontoObjetivo  = montoObjetivo,
            MontoRealizado = 0,
            Descripcion    = descripcion?.Trim(),
            Cerrado        = false
        };

        objetivo.SetCreated(userId);
        return objetivo;
    }

    public void ActualizarMontoObjetivo(decimal nuevoMonto, long? userId)
    {
        if (Cerrado)
            throw new InvalidOperationException("No se puede modificar un objetivo cerrado.");
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(nuevoMonto);
        MontoObjetivo = nuevoMonto;
        SetUpdated(userId);
    }

    public void RegistrarRealizado(decimal monto, long? userId)
    {
        MontoRealizado = monto;
        SetUpdated(userId);
    }

    public void CerrarPeriodo(long? userId)
    {
        if (Cerrado)
            throw new InvalidOperationException("El período ya está cerrado.");
        Cerrado = true;
        SetUpdated(userId);
    }
}
