using AutoMapper;
using MediatR;
using ZuluIA_Back.Application.Features.Comprobantes.DTOs;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Comprobantes.Queries;

public class GetComprobanteByIdQueryHandler(
    IComprobanteRepository repo,
    IMapper mapper)
    : IRequestHandler<GetComprobanteByIdQuery, ComprobanteDto?>
{
    public async Task<ComprobanteDto?> Handle(GetComprobanteByIdQuery request, CancellationToken ct)
    {
        var comp = await repo.GetByIdAsync(request.Id, ct);
        return comp is null ? null : mapper.Map<ComprobanteDto>(comp);
    }
}