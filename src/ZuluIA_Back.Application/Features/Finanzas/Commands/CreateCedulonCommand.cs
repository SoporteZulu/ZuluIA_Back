using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Finanzas.Commands;

public record CreateCedulonCommand(
    long TerceroId,
    long SucursalId,
    long? PlanPagoId,
    string NroCedulon,
    DateOnly FechaEmision,
    DateOnly FechaVencimiento,
    decimal Importe) : IRequest<Result<long>>;
