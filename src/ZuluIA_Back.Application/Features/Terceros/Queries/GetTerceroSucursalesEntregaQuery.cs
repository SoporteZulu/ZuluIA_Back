using MediatR;
using ZuluIA_Back.Application.Features.Terceros.DTOs;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Terceros.Queries;

public record GetTerceroSucursalesEntregaQuery(long TerceroId) : IRequest<Result<IReadOnlyList<TerceroSucursalEntregaDto>>>;
