using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.OrdenesPreparacion.Commands;

public record DespacharOrdenPreparacionCommand(
    long Id,
    long DepositoDestinoId,
    DateOnly Fecha,
    string? Observacion) : IRequest<Result<long>>;
