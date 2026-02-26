using AutoMapper;
using MediatR;
using ZuluIA_Back.Application.Features.Terceros.DTOs;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Terceros.Queries;

public class GetTerceroByIdQueryHandler(
    ITerceroRepository repo,
    IMapper mapper)
    : IRequestHandler<GetTerceroByIdQuery, TerceroDto?>
{
    public async Task<TerceroDto?> Handle(GetTerceroByIdQuery request, CancellationToken ct)
    {
        var tercero = await repo.GetByIdAsync(request.Id, ct);
        return tercero is null ? null : mapper.Map<TerceroDto>(tercero);
    }
}