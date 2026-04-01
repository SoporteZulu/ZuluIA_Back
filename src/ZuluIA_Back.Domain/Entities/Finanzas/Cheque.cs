using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Domain.Entities.Finanzas;

public class Cheque : AuditableEntity
{
    public long CajaId { get; private set; }
    public long? TerceroId { get; private set; }
    public string NroCheque { get; private set; } = string.Empty;
    public string Banco { get; private set; } = string.Empty;
    
    // Campos adicionales bancarios
    public string? CodigoSucursalBancaria { get; private set; }
    public string? CodigoPostal { get; private set; }
    public string? Plaza { get; private set; }
    public string? Cuit { get; private set; }
    public string? Titular { get; private set; }
    
    public DateOnly FechaEmision { get; private set; }
    public DateOnly FechaVencimiento { get; private set; }
    public DateOnly? FechaAcreditacion { get; private set; }
    public DateOnly? FechaDeposito { get; private set; }
    public decimal Importe { get; private set; }
    public long MonedaId { get; private set; }
    public EstadoCheque Estado { get; private set; }
    
    // Características del cheque
    public TipoCheque Tipo { get; private set; }
    public bool EsALaOrden { get; private set; }
    public bool EsCruzado { get; private set; }
    
    // Referencias
    public long? ChequeraId { get; private set; }
    public long? ComprobanteOrigenId { get; private set; }
    public long? CobroOrigenId { get; private set; }
    public long? PagoDestinoId { get; private set; }
    
    // Observaciones y conceptos
    public string? Observacion { get; private set; }
    public string? ConceptoRechazo { get; private set; }

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
        TipoCheque tipo,
        bool esALaOrden,
        bool esCruzado,
        string? titular,
        string? codigoSucursalBancaria,
        string? codigoPostal,
        long? chequeraId,
        long? comprobanteOrigenId,
        string? observacion,
        long? userId,
        string? plaza = null,
        string? cuit = null)
    {
        if (cajaId <= 0)
            throw new InvalidOperationException("La caja del cheque es obligatoria.");
        if (monedaId <= 0)
            throw new InvalidOperationException("La moneda del cheque es obligatoria.");

        ArgumentException.ThrowIfNullOrWhiteSpace(nroCheque);
        ArgumentException.ThrowIfNullOrWhiteSpace(banco);

        if (importe <= 0)
            throw new InvalidOperationException("El importe del cheque debe ser mayor a 0.");

        if (fechaVencimiento < fechaEmision)
            throw new InvalidOperationException(
                "La fecha de vencimiento no puede ser anterior a la fecha de emisión.");

        // Si es cheque propio, debe tener chequera
        if (tipo == TipoCheque.Propio && chequeraId == null)
            throw new InvalidOperationException(
                "Los cheques propios deben estar asociados a una chequera.");
        if (tipo == TipoCheque.Tercero && chequeraId.HasValue)
            throw new InvalidOperationException(
                "Los cheques de terceros no deben estar asociados a una chequera propia.");

        // Si es cheque de tercero, debería tener titular
        if (tipo == TipoCheque.Tercero && string.IsNullOrWhiteSpace(titular))
            throw new InvalidOperationException(
                "Los cheques de terceros deben tener un titular.");

        var cheque = new Cheque
        {
            CajaId                  = cajaId,
            TerceroId               = terceroId,
            NroCheque               = nroCheque.Trim(),
            Banco                   = banco.Trim(),
            CodigoSucursalBancaria  = codigoSucursalBancaria?.Trim(),
            CodigoPostal            = codigoPostal?.Trim(),
            Plaza                   = plaza?.Trim(),
            Cuit                    = cuit?.Trim(),
            Titular                 = titular?.Trim(),
            FechaEmision            = fechaEmision,
            FechaVencimiento        = fechaVencimiento,
            Importe                 = importe,
            MonedaId                = monedaId,
            Tipo                    = tipo,
            EsALaOrden              = esALaOrden,
            EsCruzado               = esCruzado,
            ChequeraId              = chequeraId,
            ComprobanteOrigenId     = comprobanteOrigenId,
            Estado                  = EstadoCheque.Cartera,
            Observacion             = observacion?.Trim()
        };

        cheque.SetCreated(userId);
        return cheque;
    }

    public static Cheque Crear(
        long cajaId,
        long? terceroId,
        string nroCheque,
        string banco,
        DateOnly fechaEmision,
        DateOnly fechaVencimiento,
        decimal importe,
        long monedaId,
        TipoCheque tipo,
        bool esALaOrden,
        bool esCruzado,
        string? titular,
        string? codigoSucursalBancaria,
        string? codigoPostal,
        long? chequeraId,
        long? comprobanteOrigenId,
        string? observacion,
        long? userId)
    {
        return Crear(
            cajaId,
            terceroId,
            nroCheque,
            banco,
            fechaEmision,
            fechaVencimiento,
            importe,
            monedaId,
            tipo,
            esALaOrden,
            esCruzado,
            titular,
            codigoSucursalBancaria,
            codigoPostal,
            chequeraId,
            comprobanteOrigenId,
            observacion,
            userId,
            null,
            null);
    }

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
        return Crear(
            cajaId,
            terceroId,
            nroCheque,
            banco,
            fechaEmision,
            fechaVencimiento,
            importe,
            monedaId,
            TipoCheque.Tercero,
            false,
            false,
            string.IsNullOrWhiteSpace(nroCheque) ? "N/D" : nroCheque,
            null,
            null,
            null,
            null,
            observacion,
            userId);
    }

    public void Depositar(DateOnly fechaDeposito, DateOnly? fechaAcreditacion, long? userId)
    {
        if (Estado != EstadoCheque.Cartera)
            throw new InvalidOperationException(
                $"Solo se pueden depositar cheques en estado Cartera. Estado actual: {Estado}.");
        if (fechaDeposito < FechaEmision)
            throw new InvalidOperationException(
                "La fecha de depósito no puede ser anterior a la fecha de emisión.");
        if (fechaAcreditacion.HasValue && fechaAcreditacion.Value < fechaDeposito)
            throw new InvalidOperationException(
                "La fecha de acreditación no puede ser anterior a la fecha de depósito.");

        Estado             = EstadoCheque.Depositado;
        FechaDeposito      = fechaDeposito;
        FechaAcreditacion  = fechaAcreditacion;
        SetUpdated(userId);
    }

    public void Rechazar(string? conceptoRechazo, string? observacion)
        => Rechazar(conceptoRechazo, observacion, null);

    public void Acreditar(DateOnly fechaAcreditacion, long? userId)
    {
        if (Estado != EstadoCheque.Depositado)
            throw new InvalidOperationException(
                $"Solo se pueden acreditar cheques depositados. Estado actual: {Estado}.");
        if (!FechaDeposito.HasValue)
            throw new InvalidOperationException("El cheque debe registrar una fecha de depósito antes de acreditarse.");
        if (fechaAcreditacion < FechaDeposito.Value)
            throw new InvalidOperationException(
                "La fecha de acreditación no puede ser anterior a la fecha de depósito.");

        Estado            = EstadoCheque.Acreditado;
        FechaAcreditacion = fechaAcreditacion;
        SetUpdated(userId);
    }

    public void Rechazar(string? conceptoRechazo, string? observacion, long? userId)
    {
        if (Estado == EstadoCheque.Rechazado)
            throw new InvalidOperationException("El cheque ya está rechazado.");
        if (Estado == EstadoCheque.Anulado || Estado == EstadoCheque.Acreditado)
            throw new InvalidOperationException(
                $"No se puede rechazar un cheque en estado {Estado}.");

        ArgumentException.ThrowIfNullOrWhiteSpace(conceptoRechazo);

        Estado          = EstadoCheque.Rechazado;
        ConceptoRechazo = conceptoRechazo?.Trim();
        Observacion     = observacion?.Trim();
        SetUpdated(userId);
    }

    public void Endosar(long nuevoTerceroId, string? observacion, long? userId)
    {
        if (Estado != EstadoCheque.Cartera)
            throw new InvalidOperationException(
                $"Solo se pueden endosar cheques en estado Cartera. Estado actual: {Estado}.");
        if (!EsALaOrden)
            throw new InvalidOperationException("Solo se pueden endosar cheques 'A la orden'.");
        if (nuevoTerceroId <= 0)
            throw new InvalidOperationException("Debe indicar el tercero destino del endoso.");

        TerceroId   = nuevoTerceroId;
        Observacion = observacion?.Trim();
        Estado      = EstadoCheque.Endosado;
        SetUpdated(userId);
    }

    public void Entregar(long? terceroId, string? observacion, long? userId)
    {
        if (Estado != EstadoCheque.Cartera)
            throw new InvalidOperationException(
                $"Solo se pueden entregar cheques en estado Cartera. Estado actual: {Estado}.");
        if (!terceroId.HasValue || terceroId <= 0)
            throw new InvalidOperationException("Debe indicar el tercero destinatario del cheque.");

        TerceroId   = terceroId;
        Observacion = observacion?.Trim();
        Estado      = EstadoCheque.Entregado;
        SetUpdated(userId);
    }

    public void Anular(string? observacion, long? userId)
    {
        if (Tipo != TipoCheque.Propio)
            throw new InvalidOperationException("Solo se pueden anular cheques propios.");
        if (Estado == EstadoCheque.Anulado)
            throw new InvalidOperationException("El cheque ya está anulado.");
        if (Estado != EstadoCheque.Cartera)
            throw new InvalidOperationException(
                $"Solo se pueden anular cheques propios en estado Cartera. Estado actual: {Estado}.");

        ArgumentException.ThrowIfNullOrWhiteSpace(observacion);

        Estado      = EstadoCheque.Anulado;
        Observacion = $"ANULADO: {observacion.Trim()}";
        SetUpdated(userId);
        SetDeleted();
    }

    public void ActualizarDatos(
        string? titular,
        DateOnly? fechaEmision,
        DateOnly? fechaVencimiento,
        string? codigoSucursalBancaria,
        string? codigoPostal,
        long? userId)
    {
        if (Estado != EstadoCheque.Cartera)
            throw new InvalidOperationException("Solo se pueden editar cheques en cartera.");

        if (!string.IsNullOrWhiteSpace(titular))
            Titular = titular.Trim();

        if (fechaEmision.HasValue)
            FechaEmision = fechaEmision.Value;

        if (fechaVencimiento.HasValue)
        {
            if (fechaVencimiento.Value < FechaEmision)
                throw new InvalidOperationException("La fecha de vencimiento no puede ser anterior a la de emisión.");

            FechaVencimiento = fechaVencimiento.Value;
        }

        if (!string.IsNullOrWhiteSpace(codigoSucursalBancaria))
            CodigoSucursalBancaria = codigoSucursalBancaria.Trim();

        if (!string.IsNullOrWhiteSpace(codigoPostal))
            CodigoPostal = codigoPostal.Trim();

        SetUpdated(userId);
    }

    public void SetObservacion(string? observacion)
    {
        Observacion = observacion?.Trim();
    }
}