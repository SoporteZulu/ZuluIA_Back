using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.Colegio;

public class PlanGeneralColegio : AuditableEntity
{
    public long SucursalId { get; private set; }
    public long PlanPagoId { get; private set; }
    public long TipoComprobanteId { get; private set; }
    public long ItemId { get; private set; }
    public long MonedaId { get; private set; }
    public string Codigo { get; private set; } = string.Empty;
    public string Descripcion { get; private set; } = string.Empty;
    public decimal ImporteBase { get; private set; }
    public bool Activo { get; private set; } = true;
    public string? Observacion { get; private set; }

    private PlanGeneralColegio() { }

    public static PlanGeneralColegio Crear(long sucursalId, long planPagoId, long tipoComprobanteId, long itemId, long monedaId, string codigo, string descripcion, decimal importeBase, string? observacion, long? userId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(codigo);
        ArgumentException.ThrowIfNullOrWhiteSpace(descripcion);
        if (importeBase <= 0)
            throw new InvalidOperationException("El importe base debe ser mayor a 0.");

        var plan = new PlanGeneralColegio
        {
            SucursalId = sucursalId,
            PlanPagoId = planPagoId,
            TipoComprobanteId = tipoComprobanteId,
            ItemId = itemId,
            MonedaId = monedaId,
            Codigo = codigo.Trim().ToUpperInvariant(),
            Descripcion = descripcion.Trim(),
            ImporteBase = importeBase,
            Observacion = observacion?.Trim(),
            Activo = true
        };

        plan.SetCreated(userId);
        return plan;
    }

    public void Actualizar(long planPagoId, long tipoComprobanteId, long itemId, long monedaId, string codigo, string descripcion, decimal importeBase, string? observacion, long? userId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(codigo);
        ArgumentException.ThrowIfNullOrWhiteSpace(descripcion);
        if (importeBase <= 0)
            throw new InvalidOperationException("El importe base debe ser mayor a 0.");

        PlanPagoId = planPagoId;
        TipoComprobanteId = tipoComprobanteId;
        ItemId = itemId;
        MonedaId = monedaId;
        Codigo = codigo.Trim().ToUpperInvariant();
        Descripcion = descripcion.Trim();
        ImporteBase = importeBase;
        Observacion = observacion?.Trim();
        SetUpdated(userId);
    }

    public void Desactivar(long? userId)
    {
        Activo = false;
        SetUpdated(userId);
    }
}
