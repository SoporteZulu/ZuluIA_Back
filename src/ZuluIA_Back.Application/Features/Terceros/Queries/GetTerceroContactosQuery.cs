using MediatR;
using ZuluIA_Back.Application.Features.Terceros.DTOs;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Terceros.Queries;

public record GetTerceroContactosQuery(long TerceroId) : IRequest<Result<IReadOnlyList<TerceroContactoDto>>>;
