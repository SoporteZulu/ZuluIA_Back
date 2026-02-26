using AutoMapper;
using MediatR;
using ZuluIA_Back.Application.Features.Contabilidad.DTOs;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Contabilidad.Queries;

public class GetAsientoByIdQueryHandler(
    IAsientoRepository repo,
    IMapper mapper)
    : IRequestHandler<GetAsientoByIdQuery, AsientoDto?>
{
    public async Task<AsientoDto?> Handle(GetAsientoByIdQuery request, CancellationToken ct)
    {
        var asiento = await repo.GetByIdAsync(request.Id, ct);
        return asiento is null ? null : mapper.Map<AsientoDto>(asiento);
    }
}