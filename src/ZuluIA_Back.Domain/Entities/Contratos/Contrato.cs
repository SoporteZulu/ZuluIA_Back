using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Domain.Entities.Contratos;

public class Contrato : AuditableEntity
{
    public long TerceroId { get; private set; }
    public long SucursalId { get; private set; }
    public long MonedaId { get; private set; }
    public string Codigo { get; private set; } = string.Empty;
    public string Descripcion { get; private set; } = string.Empty;
    public DateOnly FechaInicio { get; private set; }
    public DateOnly FechaFin { get; private set; }
    public decimal Importe { get; private set; }
    public bool RenovacionAutomatica { get; private set; }
    public EstadoContrato Estado { get; private set; }
    public string? Observacion { get; private set; }

    private Contrato() { }

    public static Contrato Crear(long terceroId, long sucursalId, long monedaId, string codigo, string descripcion, DateOnly fechaInicio, DateOnly fechaFin, decimal importe, bool renovacionAutomatica, string? observacion, long? userId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(codigo);
        ArgumentException.ThrowIfNullOrWhiteSpace(descripcion);
        if (fechaFin < fechaInicio)
            throw new InvalidOperationException("La fecha fin no puede ser anterior a la fecha inicio.");
        if (importe < 0)
            throw new InvalidOperationException("El importe del contrato no puede ser negativo.");

        var contrato = new Contrato
        {
            TerceroId = terceroId,
            SucursalId = sucursalId,
            MonedaId = monedaId,
            Codigo = codigo.Trim().ToUpperInvariant(),
            Descripcion = descripcion.Trim(),
            FechaInicio = fechaInicio,
            FechaFin = fechaFin,
            Importe = importe,
            RenovacionAutomatica = renovacionAutomatica,
            Observacion = observacion?.Trim(),
            Estado = EstadoContrato.Activo
        };

        contrato.SetCreated(userId);
        return contrato;
    }

    public void Actualizar(string descripcion, DateOnly fechaInicio, DateOnly fechaFin, decimal importe, bool renovacionAutomatica, string? observacion, long? userId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(descripcion);
        if (fechaFin < fechaInicio)
            throw new InvalidOperationException("La fecha fin no puede ser anterior a la fecha inicio.");
        if (importe < 0)
            throw new InvalidOperationException("El importe del contrato no puede ser negativo.");
        if (Estado == EstadoContrato.Cancelado)
            throw new InvalidOperationException("No se puede actualizar un contrato cancelado.");

        Descripcion = descripcion.Trim();
        FechaInicio = fechaInicio;
        FechaFin = fechaFin;
        Importe = importe;
        RenovacionAutomatica = renovacionAutomatica;
        Observacion = observacion?.Trim();
        SetUpdated(userId);
    }

    public void Renovar(DateOnly nuevaFechaFin, decimal? nuevoImporte, string? observacion, long? userId)
    {
        if (Estado == EstadoContrato.Cancelado)
            throw new InvalidOperationException("No se puede renovar un contrato cancelado.");
        if (nuevaFechaFin <= FechaFin)
            throw new InvalidOperationException("La nueva fecha fin debe ser posterior a la actual.");
        if (nuevoImporte.HasValue && nuevoImporte.Value < 0)
            throw new InvalidOperationException("El nuevo importe no puede ser negativo.");

        FechaInicio = FechaFin.AddDays(1);
        FechaFin = nuevaFechaFin;
        Importe = nuevoImporte ?? Importe;
        Observacion = observacion?.Trim() ?? Observacion;
        Estado = EstadoContrato.Renovado;
        SetUpdated(userId);
    }

    public void Cancelar(string? observacion, long? userId)
    {
        if (Estado == EstadoContrato.Cancelado)
            throw new InvalidOperationException("El contrato ya está cancelado.");

        Estado = EstadoContrato.Cancelado;
        Observacion = observacion?.Trim() ?? Observacion;
        SetUpdated(userId);
    }

    public void Finalizar(string? observacion, long? userId)
    {
        if (Estado == EstadoContrato.Cancelado)
            throw new InvalidOperationException("No se puede finalizar un contrato cancelado.");
        if (Estado == EstadoContrato.Finalizado)
            return;

        Estado = EstadoContrato.Finalizado;
        Observacion = observacion?.Trim() ?? Observacion;
        SetUpdated(userId);
    }
}
