using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Produccion.Commands;

public record CrearOrdenTrabajoCommand(
    long SucursalId,
    long FormulaId,
    long DepositoOrigenId,
    long DepositoDestinoId,
    DateOnly Fecha,
    DateOnly? FechaFinPrevista,
    decimal Cantidad,
    string? Observacion
) : IRequest<Result<long>>;