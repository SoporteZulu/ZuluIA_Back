using MediatR;
using ZuluIA_Back.Application.Features.Items.DTOs;

namespace ZuluIA_Back.Application.Features.Items.Queries;

public record GetDepositosBySucursalQuery(long SucursalId, bool IncluirInactivos = false)
    : IRequest<IReadOnlyList<DepositoDto>>;