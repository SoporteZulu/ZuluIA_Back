using AutoMapper;
using MediatR;
using ZuluIA_Back.Application.Features.Usuarios.DTOs;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Usuarios.Queries;

public class GetUsuariosPagedQueryHandler(
    IUsuarioRepository repo,
    IMapper mapper)
    : IRequestHandler<GetUsuariosPagedQuery, PagedResult<UsuarioListDto>>
{
    public async Task<PagedResult<UsuarioListDto>> Handle(
        GetUsuariosPagedQuery request,
        CancellationToken ct)
    {
        var result = await repo.GetPagedAsync(
            request.Page, request.PageSize,
            request.Search, request.SoloActivos, ct);

        var items = mapper.Map<IReadOnlyList<UsuarioListDto>>(result.Items);
        return new PagedResult<UsuarioListDto>(
            items, result.Page, result.PageSize, result.TotalCount);
    }
}