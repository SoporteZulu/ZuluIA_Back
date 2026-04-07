using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Finanzas.Commands;

public record CreatePlanTarjetaCommand(
    long TarjetaTipoId,
    string Codigo,
    string Descripcion,
    int CantidadCuotas,
    decimal Recargo,
    int DiasAcreditacion) : IRequest<Result<long>>;
