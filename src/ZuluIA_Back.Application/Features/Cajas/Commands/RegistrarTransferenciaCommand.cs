using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Cajas.Commands;

/// <summary>
/// Registra una transferencia de fondos entre dos cajas/cuentas bancarias.
/// Equivale a frmTransferenciasCajasCuentasBancarias del VB6.
/// </summary>
public record RegistrarTransferenciaCommand(
    long SucursalId,
    long CajaOrigenId,
    long CajaDestinoId,
    DateOnly Fecha,
    decimal Importe,
    long MonedaId,
    decimal Cotizacion,
    string? Concepto
) : IRequest<Result<long>>;
