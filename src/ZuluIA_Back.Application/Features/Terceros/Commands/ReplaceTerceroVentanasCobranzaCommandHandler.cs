using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Terceros.DTOs;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Terceros;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Terceros.Commands;

public class ReplaceTerceroVentanasCobranzaCommandHandler(
    IApplicationDbContext db,
    ICurrentUserService currentUser,
    IUnitOfWork uow,
    IMapper mapper) : IRequestHandler<ReplaceTerceroVentanasCobranzaCommand, Result<IReadOnlyList<TerceroVentanaCobranzaDto>>>
{
    public async Task<Result<IReadOnlyList<TerceroVentanaCobranzaDto>>> Handle(ReplaceTerceroVentanasCobranzaCommand request, CancellationToken ct)
    {
        var tercero = await db.Terceros.FirstOrDefaultAsync(x => x.Id == request.TerceroId && x.DeletedAt == null, ct);
        if (tercero is null)
            return Result.Failure<IReadOnlyList<TerceroVentanaCobranzaDto>>($"No se encontró el tercero con Id {request.TerceroId}.");

        var existentes = await db.TercerosVentanasCobranza
            .Where(x => x.TerceroId == request.TerceroId && x.DeletedAt == null)
            .OrderBy(x => x.Orden)
            .ToListAsync(ct);

        var principalIndex = request.Ventanas.Count == 0
            ? -1
            : request.Ventanas.ToList().FindIndex(x => x.Principal);

        var itemsNormalizados = request.Ventanas
            .Select((item, index) => new
            {
                Item = item,
                Orden = item.Orden ?? index,
                Principal = principalIndex == -1 ? index == 0 : item.Principal
            })
            .ToList();

        var idsSolicitados = itemsNormalizados
            .Where(x => x.Item.Id.HasValue)
            .Select(x => x.Item.Id!.Value)
            .ToHashSet();

        foreach (var item in existentes.Where(x => !idsSolicitados.Contains(x.Id)))
            item.MarcarComoEliminada(currentUser.UserId);

        foreach (var input in itemsNormalizados)
        {
            var existente = input.Item.Id.HasValue
                ? existentes.FirstOrDefault(x => x.Id == input.Item.Id.Value)
                : null;

            if (existente is null)
            {
                var nueva = TerceroVentanaCobranza.Crear(
                    request.TerceroId,
                    input.Item.Dia,
                    input.Item.Franja,
                    input.Item.Canal,
                    input.Item.Responsable,
                    input.Principal,
                    input.Orden,
                    currentUser.UserId);

                db.TercerosVentanasCobranza.Add(nueva);
                continue;
            }

            existente.Actualizar(
                input.Item.Dia,
                input.Item.Franja,
                input.Item.Canal,
                input.Item.Responsable,
                input.Principal,
                input.Orden,
                currentUser.UserId);
        }

        await uow.SaveChangesAsync(ct);

        var actualizadas = await db.TercerosVentanasCobranza
            .AsNoTracking()
            .Where(x => x.TerceroId == request.TerceroId && x.DeletedAt == null)
            .OrderByDescending(x => x.Principal)
            .ThenBy(x => x.Orden)
            .ThenBy(x => x.Dia)
            .ToListAsync(ct);

        return Result.Success<IReadOnlyList<TerceroVentanaCobranzaDto>>(mapper.Map<IReadOnlyList<TerceroVentanaCobranzaDto>>(actualizadas));
    }
}
