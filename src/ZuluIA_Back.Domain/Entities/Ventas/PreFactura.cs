using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Domain.Entities.Ventas;

/// <summary>
/// Pre-factura de venta, puede ser manual o automática, previa a la factura definitiva.
/// Migrado desde VB6: VTAPREFACTURAAUTOMATICA, VTAPREFACTURAMANUAL, VtaPreFacturaF.
/// </summary>
public class PreFactura : AuditableEntity
{
    public long         SucursalId      { get; private set; }
    public long         TerceroId       { get; private set; }
    public long?        VendedorId      { get; private set; }
    public DateOnly     Fecha           { get; private set; }
    public string       Numero          { get; private set; } = string.Empty;
    public bool         EsAutomatica    { get; private set; }
    public decimal      Subtotal        { get; private set; }
    public decimal      Impuestos       { get; private set; }
    public decimal      Total           { get; private set; }
    public EstadoPreFactura Estado      { get; private set; }
    public string?      Observacion     { get; private set; }
    public long?        ComprobanteId   { get; private set; }   // FK cuando se factura

    private readonly List<PreFacturaItem> _items = [];
    public IReadOnlyCollection<PreFacturaItem> Items => _items.AsReadOnly();

    private PreFactura() { }

    public static PreFactura Crear(
        long sucursalId, long terceroId, long? vendedorId,
        DateOnly fecha, string numero, bool esAutomatica,
        string? observacion, long? userId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(numero);
        var pf = new PreFactura
        {
            SucursalId   = sucursalId,
            TerceroId    = terceroId,
            VendedorId   = vendedorId,
            Fecha        = fecha,
            Numero       = numero.Trim(),
            EsAutomatica = esAutomatica,
            Observacion  = observacion?.Trim(),
            Estado       = EstadoPreFactura.Borrador
        };
        pf.SetCreated(userId);
        return pf;
    }

    public void AgregarItem(PreFacturaItem item) => _items.Add(item);

    public void RecalcularTotales()
    {
        Subtotal  = _items.Sum(i => i.SubTotal);
        Impuestos = _items.Sum(i => i.MontoIva);
        Total     = Subtotal + Impuestos;
    }

    public void Autorizar(long? userId)
    {
        if (Estado != EstadoPreFactura.Borrador)
            throw new InvalidOperationException("Solo se pueden autorizar pre-facturas en estado Borrador.");
        Estado = EstadoPreFactura.Autorizado;
        SetUpdated(userId);
    }

    public void Facturar(long comprobanteId, long? userId)
    {
        if (Estado != EstadoPreFactura.Autorizado)
            throw new InvalidOperationException("Solo se pueden facturar pre-facturas autorizadas.");
        Estado        = EstadoPreFactura.Facturado;
        ComprobanteId = comprobanteId;
        SetUpdated(userId);
    }

    public void Anular(long? userId)
    {
        if (Estado == EstadoPreFactura.Facturado)
            throw new InvalidOperationException("No se puede anular una pre-factura ya facturada.");
        Estado = EstadoPreFactura.Anulado;
        SetDeleted();
        SetUpdated(userId);
    }
}
