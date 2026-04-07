using MediatR;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Application.Features.Cheques.Commands;

public enum AccionCheque { Depositar, Acreditar, Rechazar, Entregar, Endosar, Anular }

public record CambiarEstadoChequeCommand(
    long Id,
    AccionCheque Accion,
    DateOnly? Fecha,
    DateOnly? FechaAcreditacion,
    string? Observacion,
    long? TerceroId = null,
    string? ConceptoRechazo = null
) : IRequest<Result>;