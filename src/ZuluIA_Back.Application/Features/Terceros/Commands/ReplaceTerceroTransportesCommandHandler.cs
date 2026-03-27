using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Terceros.DTOs;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Terceros;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Terceros.Commands;

public class ReplaceTerceroTransportesCommandHandler(
    IApplicationDbContext db,
    ICurrentUserService currentUser,
    IUnitOfWork uow,
    IMapper mapper) : IRequestHandler<ReplaceTerceroTransportesCommand, Result<IReadOnlyList<TerceroTransporteDto>>>
{
    public async Task<Result<IReadOnlyList<TerceroTransporteDto>>> Handle(ReplaceTerceroTransportesCommand request, CancellationToken ct)
    {
        var tercero = await db.Terceros.FirstOrDefaultAsync(x => x.Id == request.TerceroId && x.DeletedAt == null, ct);
        if (tercero is null)
            return Result.Failure<IReadOnlyList<TerceroTransporteDto>>($"No se encontró el tercero con Id {request.TerceroId}.");

        var transportistaIds = request.Transportes
            .Where(x => x.TransportistaId.HasValue)
            .Select(x => x.TransportistaId!.Value)
            .Distinct()
            .ToList();

        if (transportistaIds.Count > 0)
        {
            var existentesTransportistas = await db.Transportistas
                .AsNoTracking()
                .Where(x => transportistaIds.Contains(x.Id) && x.Activo)
                .Select(x => x.Id)
                .ToListAsync(ct);

            var faltantes = transportistaIds.Except(existentesTransportistas).ToList();
            if (faltantes.Count > 0)
                return Result.Failure<IReadOnlyList<TerceroTransporteDto>>("Uno o más transportistas indicados no existen o están inactivos.");
        }

        var existentes = await db.TercerosTransportes
            .Where(x => x.TerceroId == request.TerceroId && x.DeletedAt == null)
            .OrderBy(x => x.Orden)
            .ToListAsync(ct);

        var principalIndex = request.Transportes.Count == 0
            ? -1
            : request.Transportes.ToList().FindIndex(x => x.Principal);

        var itemsNormalizados = request.Transportes
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
            item.MarcarComoEliminado(currentUser.UserId);

        foreach (var input in itemsNormalizados)
        {
            var existente = input.Item.Id.HasValue
                ? existentes.FirstOrDefault(x => x.Id == input.Item.Id.Value)
                : null;

            if (existente is null)
            {
                var nuevo = TerceroTransporte.Crear(
                    request.TerceroId,
                    input.Item.TransportistaId,
                    input.Item.Nombre,
                    input.Item.Servicio,
                    input.Item.Zona,
                    input.Item.Frecuencia,
                    input.Item.Observacion,
                    input.Item.Activo,
                    input.Principal,
                    input.Orden,
                    currentUser.UserId);

                db.TercerosTransportes.Add(nuevo);
                continue;
            }

            existente.Actualizar(
                input.Item.TransportistaId,
                input.Item.Nombre,
                input.Item.Servicio,
                input.Item.Zona,
                input.Item.Frecuencia,
                input.Item.Observacion,
                input.Item.Activo,
                input.Principal,
                input.Orden,
                currentUser.UserId);
        }

        await uow.SaveChangesAsync(ct);

        var actualizados = await db.TercerosTransportes
            .AsNoTracking()
            .Where(x => x.TerceroId == request.TerceroId && x.DeletedAt == null)
            .OrderByDescending(x => x.Principal)
            .ThenBy(x => x.Orden)
            .ThenBy(x => x.Nombre)
            .ToListAsync(ct);

        var items = mapper.Map<List<TerceroTransporteDto>>(actualizados);
        var nombresTransportistas = transportistaIds.Count == 0
            ? new Dictionary<long, string>()
            : await db.Transportistas
                .AsNoTracking()
                .Where(x => transportistaIds.Contains(x.Id))
                .Join(db.Terceros.AsNoTracking(), t => t.TerceroId, ter => ter.Id, (t, ter) => new { t.Id, ter.RazonSocial })
                .ToDictionaryAsync(x => x.Id, x => x.RazonSocial, ct);

        for (var i = 0; i < items.Count; i++)
        {
            var source = actualizados[i];
            if (source.TransportistaId.HasValue && nombresTransportistas.TryGetValue(source.TransportistaId.Value, out var nombre))
                items[i].TransportistaNombre = nombre;
        }

        return Result.Success<IReadOnlyList<TerceroTransporteDto>>(items);
    }
}
