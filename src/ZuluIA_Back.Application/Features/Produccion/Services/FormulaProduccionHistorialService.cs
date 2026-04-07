using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Extensions;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Entities.Produccion;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Produccion.Services;

public class FormulaProduccionHistorialService(
    IApplicationDbContext db,
    IRepository<FormulaProduccionHistorial> historialRepo,
    ICurrentUserService currentUser)
{
    public async Task RegistrarSnapshotAsync(FormulaProduccion formula, string? motivo, CancellationToken ct = default)
    {
        var ultimaVersion = await db.FormulasProduccionHistorial.AsNoTrackingSafe()
            .Where(x => x.FormulaId == formula.Id)
            .MaxSafeAsync(x => (int?)x.Version, ct) ?? 0;

        var snapshot = JsonSerializer.Serialize(new
        {
            formula.Id,
            formula.Codigo,
            formula.Descripcion,
            formula.ItemResultadoId,
            formula.CantidadResultado,
            formula.UnidadMedidaId,
            formula.Activo,
            formula.Observacion,
            Ingredientes = formula.Ingredientes
                .OrderBy(x => x.Orden)
                .Select(x => new
                {
                    x.ItemId,
                    x.Cantidad,
                    x.UnidadMedidaId,
                    x.EsOpcional,
                    x.Orden
                })
        });

        var historial = FormulaProduccionHistorial.Registrar(
            formula.Id,
            ultimaVersion + 1,
            formula.Codigo,
            formula.Descripcion,
            formula.CantidadResultado,
            snapshot,
            motivo,
            currentUser.UserId);

        await historialRepo.AddAsync(historial, ct);
    }
}
