using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Domain.Entities.Finanzas;

public class Recibo : AuditableEntity
{
    public long SucursalId { get; private set; }
    public long TerceroId { get; private set; }
    public DateOnly Fecha { get; private set; }
    public string Serie { get; private set; } = string.Empty;
    public int Numero { get; private set; }
    public decimal Total { get; private set; }
    public string? Observacion { get; private set; }
    public EstadoRecibo Estado { get; private set; }
    public long? CobroId { get; private set; }

    // Datos comerciales
    public long? VendedorId { get; private set; }
    public long? CobradorId { get; private set; }
    public long? ZonaComercialId { get; private set; }
    public long? UsuarioCajeroId { get; private set; }

    // Snapshot de datos del tercero
    public string? TerceroCuit { get; private set; }
    public string? TerceroCondicionIva { get; private set; }
    public string? TerceroDomicilio { get; private set; }

    // Leyendas y observaciones
    public string? LeyendaFiscal { get; private set; }

    // Metadatos de impresión
    public string? FormatoImpresion { get; private set; }
    public int? CopiasImpresas { get; private set; }
    public DateTimeOffset? FechaImpresion { get; private set; }

    private readonly List<ReciboItem> _items = [];
    public IReadOnlyCollection<ReciboItem> Items => _items.AsReadOnly();

    private Recibo() { }

    public static Recibo Crear(
        long sucursalId,
        long terceroId,
        DateOnly fecha,
        string serie,
        int numero,
        string? observacion,
        long? cobroId,
        long? vendedorId,
        long? cobradorId,
        long? zonaComercialId,
        long? usuarioCajeroId,
        string? terceroCuit,
        string? terceroCondicionIva,
        string? terceroDomicilio,
        string? leyendaFiscal,
        long? userId)
    {
        var recibo = new Recibo
        {
            SucursalId              = sucursalId,
            TerceroId               = terceroId,
            Fecha                   = fecha,
            Serie                   = serie.Trim().ToUpperInvariant(),
            Numero                  = numero,
            Estado                  = EstadoRecibo.Emitido,
            Observacion             = observacion?.Trim(),
            CobroId                 = cobroId,
            VendedorId              = vendedorId,
            CobradorId              = cobradorId,
            ZonaComercialId         = zonaComercialId,
            UsuarioCajeroId         = usuarioCajeroId,
            TerceroCuit             = terceroCuit?.Trim(),
            TerceroCondicionIva     = terceroCondicionIva?.Trim(),
            TerceroDomicilio        = terceroDomicilio?.Trim(),
            LeyendaFiscal           = leyendaFiscal?.Trim(),
            Total                   = 0
        };

        recibo.SetCreated(userId);
        return recibo;
    }

    public static Recibo Crear(
        long sucursalId,
        long terceroId,
        DateOnly fecha,
        string serie,
        int numero,
        string? observacion,
        long? cobroId,
        long? userId)
    {
        return Crear(
            sucursalId,
            terceroId,
            fecha,
            serie,
            numero,
            observacion,
            cobroId,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            userId);
    }

    public void AgregarItem(ReciboItem item)
    {
        if (Estado != EstadoRecibo.Emitido)
            throw new InvalidOperationException("No se pueden agregar ítems a un recibo anulado.");

        _items.Add(item);
        RecalcularTotal();
    }

    private void RecalcularTotal() =>
        Total = _items.Sum(x => x.Importe);

    public void Anular(long? userId)
    {
        if (Estado == EstadoRecibo.Anulado)
            throw new InvalidOperationException("El recibo ya está anulado.");

        Estado = EstadoRecibo.Anulado;
        SetDeleted();
        SetUpdated(userId);
    }

    public void RegistrarImpresion(string formatoImpresion, int copias, long? userId)
    {
        FormatoImpresion = formatoImpresion;
        CopiasImpresas = (CopiasImpresas ?? 0) + copias;
        FechaImpresion = DateTimeOffset.UtcNow;
        SetUpdated(userId);
    }
}
