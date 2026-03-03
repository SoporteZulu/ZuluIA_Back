using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Cheques.Commands;

public record CreateChequeCommand(
    long CajaId,
    long? TerceroId,
    string NroCheque,
    string Banco,
    DateOnly FechaEmision,
    DateOnly FechaVencimiento,
    decimal Importe,
    long MonedaId,
    string? Observacion
) : IRequest<Result<long>>;