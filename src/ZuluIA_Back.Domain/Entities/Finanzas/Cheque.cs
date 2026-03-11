using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Domain.Entities.Finanzas;

public class Cheque : AuditableEntity
{
    public long CajaId { get; private set; }
    public long? TerceroId { get; private set; }
    public string NroCheque { get; private set; } = string.Empty;
    public string Banco { get; private set; } = string.Empty;
    public DateOnly FechaEmision { get; private set; }
    public DateOnly FechaVencimiento { get; private set; }
    public DateOnly? FechaAcreditacion { get; private set; }
    public DateOnly? FechaDeposito { get; private set; }
    public decimal Importe { get; private set; }
    public long MonedaId { get; private set; }
    public EstadoCheque Estado { get; private set; }
    public string? Observacion { get; private set; }

    private Cheque() { }

    public static Cheque Crear(
        long cajaId,
        long? terceroId,
        string nroCheque,
        string banco,
        DateOnly fechaEmision,
        DateOnly fechaVencimiento,
        decimal importe,
        long monedaId,
        string? observacion,
        long? userId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nroCheque);
        ArgumentException.ThrowIfNullOrWhiteSpace(banco);

        if (importe <= 0)
            throw new InvalidOperationException("El importe del cheque debe ser mayor a 0.");

        if (fechaVencimiento < fechaEmision)
            throw new InvalidOperationException(
                "La fecha de vencimiento no puede ser anterior a la fecha de emisión.");

        var cheque = new Cheque
        {
            CajaId           = cajaId,
            TerceroId        = terceroId,
            NroCheque        = nroCheque.Trim(),
            Banco            = banco.Trim(),
            FechaEmision     = fechaEmision,
            FechaVencimiento = fechaVencimiento,
            Importe          = importe,
            MonedaId         = monedaId,
            Estado           = EstadoCheque.Cartera,
            Observacion      = observacion?.Trim()
        };

        cheque.SetCreated(userId);
        return cheque;
    }

    public void Depositar(DateOnly fechaDeposito, DateOnly? fechaAcreditacion, long? userId)
    {
        if (Estado != EstadoCheque.Cartera)
            throw new InvalidOperationException(
                $"Solo se pueden depositar cheques en estado Cartera. Estado actual: {Estado}.");

        Estado             = EstadoCheque.Depositado;
        FechaDeposito      = fechaDeposito;
        FechaAcreditacion  = fechaAcreditacion;
        SetUpdated(userId);
    }

    public void Acreditar(DateOnly fechaAcreditacion, long? userId)
    {
        if (Estado != EstadoCheque.Depositado)
            throw new InvalidOperationException(
                $"Solo se pueden acreditar cheques depositados. Estado actual: {Estado}.");

        Estado            = EstadoCheque.Acreditado;
        FechaAcreditacion = fechaAcreditacion;
        SetUpdated(userId);
    }

    public void Rechazar(string? observacion, long? userId)
    {
        if (Estado == EstadoCheque.Rechazado)
            throw new InvalidOperationException("El cheque ya está rechazado.");

        Estado      = EstadoCheque.Rechazado;
        Observacion = observacion?.Trim();
        SetUpdated(userId);
    }

    public void Entregar(long? userId)
    {
        if (Estado != EstadoCheque.Cartera)
            throw new InvalidOperationException(
                $"Solo se pueden entregar cheques en estado Cartera. Estado actual: {Estado}.");

        Estado = EstadoCheque.Entregado;
        SetUpdated(userId);
    }

    public void SetObservacion(string? obs) => Observacion = obs?.Trim();
}