using AutoMapper;
using MediatR;
using ZuluIA_Back.Application.Features.Configuracion.DTOs;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Configuracion.Queries;

public class GetConfiguracionQueryHandler(
    IConfiguracionRepository repo,
    IMapper mapper)
    : IRequestHandler<GetConfiguracionQuery, IReadOnlyList<ConfiguracionDto>>
{
    public async Task<IReadOnlyList<ConfiguracionDto>> Handle(
        GetConfiguracionQuery request,
        CancellationToken ct)
    {
        var items = await repo.GetAllAsync(ct);
        return mapper.Map<IReadOnlyList<ConfiguracionDto>>(items);
    }
}