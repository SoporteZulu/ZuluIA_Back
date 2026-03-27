using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Cheques.Commands;

public enum AccionCheque { Depositar, Acreditar, Rechazar, Entregar }

public record CambiarEstadoChequeCommand(
    long Id,
    AccionCheque Accion,
    DateOnly? Fecha,
    DateOnly? FechaAcreditacion,
    string? Observacion,
    long? TerceroId = null
) : IRequest<Result>;