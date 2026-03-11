using AutoMapper;
using MediatR;
using ZuluIA_Back.Application.Features.Items.DTOs;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Items.Queries;

public class GetCategoriasItemsQueryHandler(
    ICategoriaItemRepository repo,
    IMapper mapper)
    : IRequestHandler<GetCategoriasItemsQuery, IReadOnlyList<CategoriaItemDto>>
{
    public async Task<IReadOnlyList<CategoriaItemDto>> Handle(
        GetCategoriasItemsQuery request,
        CancellationToken ct)
    {
        var items = await repo.GetArbolCompletoAsync(ct);
        var dtos = mapper.Map<IReadOnlyList<CategoriaItemDto>>(items);
        return BuildTree(dtos);
    }

    private static IReadOnlyList<CategoriaItemDto> BuildTree(
        IReadOnlyList<CategoriaItemDto> flat)
    {
        var lookup = flat.ToDictionary(x => x.Id);
        var roots = new List<CategoriaItemDto>();

        foreach (var item in flat)
        {
            if (item.ParentId.HasValue &&
                lookup.TryGetValue(item.ParentId.Value, out var parent))
                ((List<CategoriaItemDto>)parent.Hijos).Add(item);
            else
                roots.Add(item);
        }

        return roots
            .OrderBy(x => x.OrdenNivel)
            .ThenBy(x => x.Descripcion)
            .ToList()
            .AsReadOnly();
    }
}