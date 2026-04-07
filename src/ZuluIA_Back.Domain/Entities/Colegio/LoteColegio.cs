using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Domain.Entities.Colegio;

public class LoteColegio : AuditableEntity
{
    public long PlanGeneralColegioId { get; private set; }
    public string Codigo { get; private set; } = string.Empty;
    public DateOnly FechaEmision { get; private set; }
    public DateOnly FechaVencimiento { get; private set; }
    public EstadoLoteColegio Estado { get; private set; }
    public int CantidadCedulones { get; private set; }
    public string? Observacion { get; private set; }

    private LoteColegio() { }

    public static LoteColegio Crear(long planGeneralColegioId, string codigo, DateOnly fechaEmision, DateOnly fechaVencimiento, string? observacion, long? userId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(codigo);
        if (fechaVencimiento < fechaEmision)
            throw new InvalidOperationException("La fecha de vencimiento no puede ser anterior a la de emisión.");

        var lote = new LoteColegio
        {
            PlanGeneralColegioId = planGeneralColegioId,
            Codigo = codigo.Trim().ToUpperInvariant(),
            FechaEmision = fechaEmision,
            FechaVencimiento = fechaVencimiento,
            Estado = EstadoLoteColegio.Borrador,
            Observacion = observacion?.Trim()
        };

        lote.SetCreated(userId);
        return lote;
    }

    public void Actualizar(DateOnly fechaEmision, DateOnly fechaVencimiento, string? observacion, long? userId)
    {
        if (Estado != EstadoLoteColegio.Borrador)
            throw new InvalidOperationException("Solo se pueden editar lotes en estado borrador.");
        if (fechaVencimiento < fechaEmision)
            throw new InvalidOperationException("La fecha de vencimiento no puede ser anterior a la de emisión.");

        FechaEmision = fechaEmision;
        FechaVencimiento = fechaVencimiento;
        Observacion = observacion?.Trim();
        SetUpdated(userId);
    }

    public void MarcarEmitido(int cantidadCedulones, long? userId)
    {
        if (cantidadCedulones <= 0)
            throw new InvalidOperationException("La cantidad de cedulones debe ser mayor a 0.");

        CantidadCedulones = cantidadCedulones;
        Estado = EstadoLoteColegio.Emitido;
        SetUpdated(userId);
    }

    public void MarcarFacturado(long? userId)
    {
        Estado = EstadoLoteColegio.Facturado;
        SetUpdated(userId);
    }

    public void Cerrar(string? observacion, long? userId)
    {
        if (Estado == EstadoLoteColegio.Borrador)
            throw new InvalidOperationException("No se puede cerrar un lote de colegio sin haber emitido cedulones.");

        Estado = EstadoLoteColegio.Cerrado;
        Observacion = observacion?.Trim() ?? Observacion;
        SetUpdated(userId);
    }
}
