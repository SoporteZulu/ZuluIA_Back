using MediatR;
using ZuluIA_Back.Application.Features.Terceros.DTOs;

namespace ZuluIA_Back.Application.Features.Terceros.Queries;

public record GetTerceroByIdQuery(long Id) : IRequest<TerceroDto?>;