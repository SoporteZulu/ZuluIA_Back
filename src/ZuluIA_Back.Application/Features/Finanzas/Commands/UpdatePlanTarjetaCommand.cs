using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Finanzas.Commands;

public record UpdatePlanTarjetaCommand(
    long Id,
    string Descripcion,
    int CantidadCuotas,
    decimal Recargo,
    int DiasAcreditacion) : IRequest<Result>;
