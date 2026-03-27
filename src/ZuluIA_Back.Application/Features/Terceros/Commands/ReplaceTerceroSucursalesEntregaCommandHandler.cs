using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Terceros.DTOs;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Terceros;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Terceros.Commands;

public class ReplaceTerceroSucursalesEntregaCommandHandler(
    IApplicationDbContext db,
    ICurrentUserService currentUser,
    IUnitOfWork uow,
    IMapper mapper) : IRequestHandler<ReplaceTerceroSucursalesEntregaCommand, Result<IReadOnlyList<TerceroSucursalEntregaDto>>>
{
    public async Task<Result<IReadOnlyList<TerceroSucursalEntregaDto>>> Handle(ReplaceTerceroSucursalesEntregaCommand request, CancellationToken ct)
    {
        var tercero = await db.Terceros.FirstOrDefaultAsync(x => x.Id == request.TerceroId && x.DeletedAt == null, ct);
        if (tercero is null)
            return Result.Failure<IReadOnlyList<TerceroSucursalEntregaDto>>($"No se encontró el tercero con Id {request.TerceroId}.");

        var existentes = await db.TercerosSucursalesEntrega
            .Where(x => x.TerceroId == request.TerceroId && x.DeletedAt == null)
            .OrderBy(x => x.Orden)
            .ToListAsync(ct);

        var principalIndex = request.Sucursales.Count == 0
            ? -1
            : request.Sucursales.ToList().FindIndex(x => x.Principal);

        var itemsNormalizados = request.Sucursales
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
                var nueva = TerceroSucursalEntrega.Crear(
                    request.TerceroId,
                    input.Item.Descripcion,
                    input.Item.Direccion,
                    input.Item.Localidad,
                    input.Item.Responsable,
                    input.Item.Telefono,
                    input.Item.Horario,
                    input.Principal,
                    input.Orden,
                    currentUser.UserId);

                db.TercerosSucursalesEntrega.Add(nueva);
                continue;
            }

            existente.Actualizar(
                input.Item.Descripcion,
                input.Item.Direccion,
                input.Item.Localidad,
                input.Item.Responsable,
                input.Item.Telefono,
                input.Item.Horario,
                input.Principal,
                input.Orden,
                currentUser.UserId);
        }

        await uow.SaveChangesAsync(ct);

        var actualizadas = await db.TercerosSucursalesEntrega
            .AsNoTracking()
            .Where(x => x.TerceroId == request.TerceroId && x.DeletedAt == null)
            .OrderByDescending(x => x.Principal)
            .ThenBy(x => x.Orden)
            .ThenBy(x => x.Descripcion)
            .ToListAsync(ct);

        return Result.Success<IReadOnlyList<TerceroSucursalEntregaDto>>(mapper.Map<IReadOnlyList<TerceroSucursalEntregaDto>>(actualizadas));
    }
}
