using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Produccion.Commands;

public record RegistrarAjusteProduccionCommand(
    long FormulaId,
    long DepositoOrigenId,
    long DepositoDestinoId,
    decimal Cantidad,
    string? Observacion
) : IRequest<Result>;
