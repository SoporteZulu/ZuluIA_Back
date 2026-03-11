using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Items.Commands;

public record CreateDepositoCommand(
    long SucursalId,
    string Descripcion,
    bool EsDefault
) : IRequest<Result<long>>;