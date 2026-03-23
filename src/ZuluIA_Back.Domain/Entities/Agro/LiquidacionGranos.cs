using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Domain.Entities.Agro;

public class LiquidacionGranos : AuditableEntity
{
    public long SucursalId { get; private set; }
    public long TerceroId { get; private set; }
    public string Producto { get; private set; } = string.Empty;
    public DateOnly Fecha { get; private set; }
    public decimal Cantidad { get; private set; }
    public decimal PrecioBase { get; private set; }
    public decimal Deducciones { get; private set; }
    public decimal ValorNeto { get; private set; }
    public EstadoLiquidacionGranos Estado { get; private set; }
    public long? ComprobanteId { get; private set; }

    private readonly List<LiquidacionGranosConcepto> _conceptos = [];
    public IReadOnlyCollection<LiquidacionGranosConcepto> Conceptos => _conceptos.AsReadOnly();

    private LiquidacionGranos() { }

    public static LiquidacionGranos Crear(
        long sucursalId,
        long terceroId,
        string producto,
        DateOnly fecha,
        decimal cantidad,
        decimal precioBase,
        long? userId)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(cantidad);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(precioBase);
        var liq = new LiquidacionGranos
        {
            SucursalId  = sucursalId,
            TerceroId   = terceroId,
            Producto    = producto.Trim(),
            Fecha       = fecha,
            Cantidad    = cantidad,
            PrecioBase  = precioBase,
            Deducciones = 0,
            ValorNeto   = cantidad * precioBase,
            Estado      = EstadoLiquidacionGranos.Borrador
        };

        liq.SetCreated(userId);
        return liq;
    }

    public void AgregarConcepto(LiquidacionGranosConcepto concepto)
    {
        if (Estado != EstadoLiquidacionGranos.Borrador)
            throw new InvalidOperationException("Solo se pueden agregar conceptos a liquidaciones en borrador.");
        _conceptos.Add(concepto);
        RecalcularValores();
    }

    private void RecalcularValores()
    {
        Deducciones = _conceptos.Where(c => c.EsDeduccion).Sum(c => c.Importe);
        var adicionales = _conceptos.Where(c => !c.EsDeduccion).Sum(c => c.Importe);
        ValorNeto = Cantidad * PrecioBase - Deducciones + adicionales;
    }

    public void Emitir(long? userId)
    {
        if (Estado != EstadoLiquidacionGranos.Borrador)
            throw new InvalidOperationException("Solo se pueden emitir liquidaciones en borrador.");
        Estado = EstadoLiquidacionGranos.Emitida;
        SetUpdated(userId);
    }

    public void Anular(long? userId)
    {
        if (Estado == EstadoLiquidacionGranos.Anulada)
            throw new InvalidOperationException("La liquidación ya está anulada.");
        Estado = EstadoLiquidacionGranos.Anulada;
        SetDeleted();
        SetUpdated(userId);
    }

    public void AsignarComprobante(long comprobanteId, long? userId)
    {
        ComprobanteId = comprobanteId;
        SetUpdated(userId);
    }
}
