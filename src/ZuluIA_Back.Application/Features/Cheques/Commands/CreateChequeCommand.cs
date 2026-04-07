using MediatR;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Application.Features.Cheques.Commands;

public record CreateChequeCommand : IRequest<Result<long>>
{
    public long CajaId { get; init; }
    public long? TerceroId { get; init; }
    public string NroCheque { get; init; } = string.Empty;
    public string Banco { get; init; } = string.Empty;
    public DateOnly FechaEmision { get; init; }
    public DateOnly FechaVencimiento { get; init; }
    public decimal Importe { get; init; }
    public long MonedaId { get; init; }
    public TipoCheque Tipo { get; init; }
    public bool EsALaOrden { get; init; }
    public bool EsCruzado { get; init; }
    public string? Titular { get; init; }
    public string? Cuit { get; init; }
    public string? Plaza { get; init; }
    public string? CodigoSucursalBancaria { get; init; }
    public string? CodigoPostal { get; init; }
    public long? ChequeraId { get; init; }
    public long? ComprobanteOrigenId { get; init; }
    public long? CobroOrigenId { get; init; }
    public string? Observacion { get; init; }

    public CreateChequeCommand(
        long CajaId,
        long? TerceroId,
        string NroCheque,
        string Banco,
        DateOnly FechaEmision,
        DateOnly FechaVencimiento,
        decimal Importe,
        long MonedaId,
        TipoCheque Tipo,
        bool EsALaOrden,
        bool EsCruzado,
        string? Titular,
        string? Cuit,
        string? Plaza,
        string? CodigoSucursalBancaria,
        string? CodigoPostal,
        long? ChequeraId,
        long? ComprobanteOrigenId,
        long? CobroOrigenId = null,
        string? Observacion = null)
    {
        this.CajaId = CajaId;
        this.TerceroId = TerceroId;
        this.NroCheque = NroCheque;
        this.Banco = Banco;
        this.FechaEmision = FechaEmision;
        this.FechaVencimiento = FechaVencimiento;
        this.Importe = Importe;
        this.MonedaId = MonedaId;
        this.Tipo = Tipo;
        this.EsALaOrden = EsALaOrden;
        this.EsCruzado = EsCruzado;
        this.Titular = Titular;
        this.Cuit = Cuit;
        this.Plaza = Plaza;
        this.CodigoSucursalBancaria = CodigoSucursalBancaria;
        this.CodigoPostal = CodigoPostal;
        this.ChequeraId = ChequeraId;
        this.ComprobanteOrigenId = ComprobanteOrigenId;
        this.CobroOrigenId = CobroOrigenId;
        this.Observacion = Observacion;
    }

    public CreateChequeCommand(
        long CajaId,
        long? TerceroId,
        string NroCheque,
        string Banco,
        DateOnly FechaEmision,
        DateOnly FechaVencimiento,
        decimal Importe,
        long MonedaId,
        TipoCheque Tipo,
        bool EsALaOrden,
        bool EsCruzado,
        string? Titular,
        string? Cuit,
        string? Plaza,
        string? CodigoSucursalBancaria,
        string? CodigoPostal,
        long? ChequeraId)
        : this(
            CajaId,
            TerceroId,
            NroCheque,
            Banco,
            FechaEmision,
            FechaVencimiento,
            Importe,
            MonedaId,
            Tipo,
            EsALaOrden,
            EsCruzado,
            Titular,
            Cuit,
            Plaza,
            CodigoSucursalBancaria,
            CodigoPostal,
            ChequeraId,
            null,
            null)
    {
    }

    public CreateChequeCommand(
        long cajaId,
        long? terceroId,
        string nroCheque,
        string banco,
        DateOnly fechaEmision,
        DateOnly fechaVencimiento,
        decimal importe,
        long monedaId,
        string? observacion)
        : this(
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
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            observacion)
    {
    }
}
