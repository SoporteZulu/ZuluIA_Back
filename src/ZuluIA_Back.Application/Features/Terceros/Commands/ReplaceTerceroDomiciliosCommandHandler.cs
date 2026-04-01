using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Terceros.DTOs;
using ZuluIA_Back.Application.Features.Terceros.Queries;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Terceros;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Terceros.Commands;

public class ReplaceTerceroDomiciliosCommandHandler(
    IApplicationDbContext db,
    IUnitOfWork uow,
    IMapper mapper) : IRequestHandler<ReplaceTerceroDomiciliosCommand, Result<IReadOnlyList<TerceroDomicilioDto>>>
{
    public async Task<Result<IReadOnlyList<TerceroDomicilioDto>>> Handle(ReplaceTerceroDomiciliosCommand request, CancellationToken ct)
    {
        var tercero = await db.Terceros.FirstOrDefaultAsync(x => x.Id == request.TerceroId && x.DeletedAt == null, ct);
        if (tercero is null)
            return Result.Failure<IReadOnlyList<TerceroDomicilioDto>>($"No se encontró el tercero con Id {request.TerceroId}.");

        if (request.Domicilios.Count > PersonaDomicilio.MaxCantidadPorPersona)
            return Result.Failure<IReadOnlyList<TerceroDomicilioDto>>($"Se permiten hasta {PersonaDomicilio.MaxCantidadPorPersona} domicilios por tercero.");

        var provinciaIds = request.Domicilios
            .Where(x => x.ProvinciaId.HasValue)
            .Select(x => x.ProvinciaId!.Value)
            .Distinct()
            .ToList();

        if (provinciaIds.Count > 0)
        {
            var provinciasExistentes = await db.Provincias
                .AsNoTracking()
                .Where(x => provinciaIds.Contains(x.Id))
                .Select(x => x.Id)
                .ToListAsync(ct);

            if (provinciaIds.Except(provinciasExistentes).Any())
                return Result.Failure<IReadOnlyList<TerceroDomicilioDto>>("Una o más provincias indicadas no existen.");
        }

        var localidadIds = request.Domicilios
            .Where(x => x.LocalidadId.HasValue)
            .Select(x => x.LocalidadId!.Value)
            .Distinct()
            .ToList();

        if (localidadIds.Count > 0)
        {
            var localidadesExistentes = await db.Localidades
                .AsNoTracking()
                .Where(x => localidadIds.Contains(x.Id))
                .Select(x => x.Id)
                .ToListAsync(ct);

            if (localidadIds.Except(localidadesExistentes).Any())
                return Result.Failure<IReadOnlyList<TerceroDomicilioDto>>("Una o más localidades indicadas no existen.");
        }

        var existentes = await db.PersonasDomicilios
            .Where(x => x.PersonaId == request.TerceroId)
            .OrderBy(x => x.Orden)
            .ToListAsync(ct);

        var defectoIndex = request.Domicilios.Count == 0
            ? -1
            : request.Domicilios.ToList().FindIndex(x => x.EsDefecto);

        var itemsNormalizados = request.Domicilios
            .Select((item, index) => new
            {
                Item = item,
                Orden = item.Orden ?? index,
                EsDefecto = defectoIndex == -1 ? index == 0 : item.EsDefecto
            })
            .ToList();

        var idsSolicitados = itemsNormalizados
            .Where(x => x.Item.Id.HasValue)
            .Select(x => x.Item.Id!.Value)
            .ToHashSet();

        foreach (var domicilio in existentes.Where(x => !idsSolicitados.Contains(x.Id)).ToList())
            db.PersonasDomicilios.Remove(domicilio);

        foreach (var input in itemsNormalizados)
        {
            var provinciaResult = await TerceroGeografiaRules.ResolveProvinciaIdAsync(
                db,
                input.Item.ProvinciaId,
                input.Item.LocalidadId,
                null,
                ct);

            if (provinciaResult.IsFailure)
                return Result.Failure<IReadOnlyList<TerceroDomicilioDto>>(provinciaResult.Error!);

            var existente = input.Item.Id.HasValue
                ? existentes.FirstOrDefault(x => x.Id == input.Item.Id.Value)
                : null;

            if (existente is null)
            {
                var nuevo = PersonaDomicilio.Crear(
                    request.TerceroId,
                    input.Item.TipoDomicilioId,
                    provinciaResult.Value,
                    input.Item.LocalidadId,
                    Normalize(input.Item.Calle),
                    Normalize(input.Item.Barrio),
                    Normalize(input.Item.CodigoPostal),
                    Normalize(input.Item.Observacion),
                    input.Orden,
                    input.EsDefecto);

                db.PersonasDomicilios.Add(nuevo);
                continue;
            }

            existente.Actualizar(
                input.Item.TipoDomicilioId,
                provinciaResult.Value,
                input.Item.LocalidadId,
                Normalize(input.Item.Calle),
                Normalize(input.Item.Barrio),
                Normalize(input.Item.CodigoPostal),
                Normalize(input.Item.Observacion),
                input.Orden,
                input.EsDefecto);
        }

        tercero.SetDomicilio(await TerceroDomicilioPrincipalSync.ResolvePrincipalAsync(db, tercero.Domicilio, request.Domicilios, ct));

        await uow.SaveChangesAsync(ct);

        var actualizados = await db.PersonasDomicilios
            .AsNoTracking()
            .Where(x => x.PersonaId == request.TerceroId)
            .OrderByDescending(x => x.EsDefecto)
            .ThenBy(x => x.Orden)
            .ThenBy(x => x.Calle)
            .ToListAsync(ct);

        var dtos = mapper.Map<List<TerceroDomicilioDto>>(actualizados);
        await TerceroDomicilioReadModelLoader.LoadDescripcionesAsync(db, dtos, ct);
        return Result.Success<IReadOnlyList<TerceroDomicilioDto>>(dtos);
    }

    private static string? Normalize(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
