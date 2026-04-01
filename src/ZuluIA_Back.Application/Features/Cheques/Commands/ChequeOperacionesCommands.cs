using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Cheques.Commands;

public record EndosarChequeCommand(
    long ChequeId,
    long NuevoTerceroId,
    string? Observacion
) : IRequest<Result>;

public record AnularChequePropioCommand(
    long ChequeId,
    string Motivo
) : IRequest<Result>;

public record ActualizarChequeCommand(
    long ChequeId,
    string? Titular,
    DateOnly? FechaEmision,
    DateOnly? FechaVencimiento,
    string? CodigoSucursalBancaria,
    string? CodigoPostal
) : IRequest<Result>;
