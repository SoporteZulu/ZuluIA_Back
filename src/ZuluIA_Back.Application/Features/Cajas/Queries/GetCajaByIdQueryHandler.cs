using AutoMapper;
using MediatR;
using ZuluIA_Back.Application.Features.Cajas.DTOs;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Cajas.Queries;

public class GetCajaByIdQueryHandler(
    ICajaRepository repo,
    IMapper mapper)
    : IRequestHandler<GetCajaByIdQuery, CajaDto?>
{
    public async Task<CajaDto?> Handle(GetCajaByIdQuery request, CancellationToken ct)
    {
        var caja = await repo.GetByIdAsync(request.Id, ct);
        return caja is null ? null : mapper.Map<CajaDto>(caja);
    }
}