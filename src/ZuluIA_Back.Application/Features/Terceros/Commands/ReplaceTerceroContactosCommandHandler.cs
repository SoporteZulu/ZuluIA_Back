using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Terceros.DTOs;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Terceros;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Terceros.Commands;

public class ReplaceTerceroContactosCommandHandler(
    IApplicationDbContext db,
    ICurrentUserService currentUser,
    IUnitOfWork uow,
    IMapper mapper) : IRequestHandler<ReplaceTerceroContactosCommand, Result<IReadOnlyList<TerceroContactoDto>>>
{
    public async Task<Result<IReadOnlyList<TerceroContactoDto>>> Handle(ReplaceTerceroContactosCommand request, CancellationToken ct)
    {
        var tercero = await db.Terceros.FirstOrDefaultAsync(x => x.Id == request.TerceroId && x.DeletedAt == null, ct);
        if (tercero is null)
            return Result.Failure<IReadOnlyList<TerceroContactoDto>>($"No se encontró el tercero con Id {request.TerceroId}.");

        var existentes = await db.TercerosContactos
            .Where(x => x.TerceroId == request.TerceroId && x.DeletedAt == null)
            .OrderBy(x => x.Orden)
            .ToListAsync(ct);

        var principalIndex = request.Contactos.Count == 0
            ? -1
            : request.Contactos.ToList().FindIndex(x => x.Principal);

        var contactosNormalizados = request.Contactos
            .Select((item, index) => new
            {
                Item = item,
                Orden = item.Orden ?? index,
                Principal = principalIndex == -1 ? index == 0 : item.Principal
            })
            .ToList();

        var idsSolicitados = contactosNormalizados
            .Where(x => x.Item.Id.HasValue)
            .Select(x => x.Item.Id!.Value)
            .ToHashSet();

        foreach (var contacto in existentes.Where(x => !idsSolicitados.Contains(x.Id)))
            contacto.MarcarComoEliminado(currentUser.UserId);

        foreach (var contactoInput in contactosNormalizados)
        {
            var existente = contactoInput.Item.Id.HasValue
                ? existentes.FirstOrDefault(x => x.Id == contactoInput.Item.Id.Value)
                : null;

            if (existente is null)
            {
                var nuevo = TerceroContacto.Crear(
                    request.TerceroId,
                    contactoInput.Item.Nombre,
                    contactoInput.Item.Cargo,
                    contactoInput.Item.Email,
                    contactoInput.Item.Telefono,
                    contactoInput.Item.Sector,
                    contactoInput.Principal,
                    contactoInput.Orden,
                    currentUser.UserId);

                db.TercerosContactos.Add(nuevo);
                continue;
            }

            existente.Actualizar(
                contactoInput.Item.Nombre,
                contactoInput.Item.Cargo,
                contactoInput.Item.Email,
                contactoInput.Item.Telefono,
                contactoInput.Item.Sector,
                contactoInput.Principal,
                contactoInput.Orden,
                currentUser.UserId);
        }

        await uow.SaveChangesAsync(ct);

        var actualizados = await db.TercerosContactos
            .AsNoTracking()
            .Where(x => x.TerceroId == request.TerceroId && x.DeletedAt == null)
            .OrderByDescending(x => x.Principal)
            .ThenBy(x => x.Orden)
            .ThenBy(x => x.Nombre)
            .ToListAsync(ct);

        return Result.Success<IReadOnlyList<TerceroContactoDto>>(mapper.Map<IReadOnlyList<TerceroContactoDto>>(actualizados));
    }
}
