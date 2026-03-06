using MediatR;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Produccion.DTOs;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Produccion.Queries;

public class GetFormulasProduccionQueryHandler(
    IFormulaProduccionRepository repo,
    IApplicationDbContext db)
    : IRequestHandler<GetFormulasProduccionQuery, IReadOnlyList<FormulaProduccionDto>>
{
    public async Task<IReadOnlyList<FormulaProduccionDto>> Handle(
        GetFormulasProduccionQuery request,
        CancellationToken ct)
    {
        var formulas = request.SoloActivas
            ? await repo.GetActivasAsync(ct)
            : await db.FormulasProduccion.AsNoTracking()
                .OrderBy(x => x.Codigo)
                .ToListAsync(ct);

        var itemIds = formulas
            .SelectMany(f => f.Ingredientes.Select(i => i.ItemId))
            .Concat(formulas.Select(f => f.ItemResultadoId))
            .Distinct().ToList();

        var umIds = formulas
            .Where(f => f.UnidadMedidaId.HasValue)
            .Select(f => f.UnidadMedidaId!.Value)
            .Concat(formulas.SelectMany(f =>
                f.Ingredientes
                 .Where(i => i.UnidadMedidaId.HasValue)
                 .Select(i => i.UnidadMedidaId!.Value)))
            .Distinct().ToList();

        var items = await db.Items.AsNoTracking()
            .Where(x => itemIds.Contains(x.Id))
            .Select(x => new { x.Id, x.Codigo, x.Descripcion })
            .ToDictionaryAsync(x => x.Id, ct);

        var unidades = await db.UnidadesMedida.AsNoTracking()
            .Where(x => umIds.Contains(x.Id))
            .Select(x => new { x.Id, x.Descripcion })
            .ToDictionaryAsync(x => x.Id, ct);

        return formulas.Select(f => new FormulaProduccionDto
        {
            Id                       = f.Id,
            Codigo                   = f.Codigo,
            Descripcion              = f.Descripcion,
            ItemResultadoId          = f.ItemResultadoId,
            ItemResultadoCodigo      = items.GetValueOrDefault(f.ItemResultadoId)?.Codigo      ?? "—",
            ItemResultadoDescripcion = items.GetValueOrDefault(f.ItemResultadoId)?.Descripcion ?? "—",
            CantidadResultado        = f.CantidadResultado,
            UnidadMedidaId           = f.UnidadMedidaId,
            UnidadMedidaDescripcion  = f.UnidadMedidaId.HasValue
                ? unidades.GetValueOrDefault(f.UnidadMedidaId.Value)?.Descripcion : null,
            Activo                   = f.Activo,
            Observacion              = f.Observacion,
            Ingredientes = f.Ingredientes
                .OrderBy(i => i.Orden)
                .Select(i => new FormulaIngredienteDto
                {
                    Id                      = i.Id,
                    ItemId                  = i.ItemId,
                    ItemCodigo              = items.GetValueOrDefault(i.ItemId)?.Codigo      ?? "—",
                    ItemDescripcion         = items.GetValueOrDefault(i.ItemId)?.Descripcion ?? "—",
                    Cantidad                = i.Cantidad,
                    UnidadMedidaId          = i.UnidadMedidaId,
                    UnidadMedidaDescripcion = i.UnidadMedidaId.HasValue
                        ? unidades.GetValueOrDefault(i.UnidadMedidaId.Value)?.Descripcion : null,
                    EsOpcional              = i.EsOpcional,
                    Orden                   = i.Orden
                }).ToList().AsReadOnly()
        }).ToList().AsReadOnly();
    }
}