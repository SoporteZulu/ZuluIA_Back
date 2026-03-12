using AutoMapper;
using MediatR;
using ZuluIA_Back.Application.Features.Usuarios.DTOs;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Usuarios.Queries;

public class GetMenuUsuarioQueryHandler(
    IMenuRepository menuRepo,
    IMapper mapper)
    : IRequestHandler<GetMenuUsuarioQuery, IReadOnlyList<MenuItemDto>>
{
    public async Task<IReadOnlyList<MenuItemDto>> Handle(
        GetMenuUsuarioQuery request,
        CancellationToken ct)
    {
        var items = await menuRepo.GetMenuUsuarioAsync(request.UsuarioId, ct);
        var dtos = mapper.Map<IReadOnlyList<MenuItemDto>>(items);

        // Construir árbol jerárquico
        return BuildTree(dtos);
    }

    private static IReadOnlyList<MenuItemDto> BuildTree(
        IReadOnlyList<MenuItemDto> flatList)
    {
        var lookup = flatList.ToDictionary(x => x.Id);
        var roots = new List<MenuItemDto>();

        foreach (var item in flatList)
        {
            if (item.ParentId.HasValue && lookup.TryGetValue(item.ParentId.Value, out var parent))
                parent.Hijos.Add(item);
            else
                roots.Add(item);
        }

        return roots.OrderBy(x => x.Orden).ToList().AsReadOnly();
    }
}