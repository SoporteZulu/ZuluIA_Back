using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Entities.Terceros;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Application.Features.Terceros.Commands;

internal static class TerceroAggregatePersistence
{
    public static async Task SyncPrincipalDomicilioAsync(
        IApplicationDbContext db,
        long terceroId,
        Domain.ValueObjects.Domicilio domicilioPrincipal,
        CancellationToken ct)
    {
        var domicilioDefault = await TerceroDomicilioPrincipalSync.BuildDefaultFromPrincipalAsync(db, domicilioPrincipal, ct);
        if (domicilioDefault is null)
            return;

        var existente = await db.PersonasDomicilios
            .Where(x => x.PersonaId == terceroId)
            .OrderByDescending(x => x.EsDefecto)
            .ThenBy(x => x.Orden)
            .FirstOrDefaultAsync(ct);

        if (existente is null)
        {
            db.PersonasDomicilios.Add(PersonaDomicilio.Crear(
                terceroId,
                domicilioDefault.TipoDomicilioId,
                domicilioDefault.ProvinciaId,
                domicilioDefault.LocalidadId,
                Normalize(domicilioDefault.Calle),
                Normalize(domicilioDefault.Barrio),
                Normalize(domicilioDefault.CodigoPostal),
                Normalize(domicilioDefault.Observacion),
                domicilioDefault.Orden ?? 0,
                true));
            return;
        }

        var tipoDomicilioId = domicilioDefault.TipoDomicilioId ?? existente.TipoDomicilioId;

        existente.Actualizar(
            tipoDomicilioId,
            domicilioDefault.ProvinciaId,
            domicilioDefault.LocalidadId,
            Normalize(domicilioDefault.Calle),
            Normalize(domicilioDefault.Barrio),
            Normalize(domicilioDefault.CodigoPostal),
            Normalize(domicilioDefault.Observacion),
            domicilioDefault.Orden ?? existente.Orden,
            true);
    }

    public static async Task<string?> ApplyOptionalSectionsAsync(
        IApplicationDbContext db,
        long terceroId,
        long? userId,
        TerceroPerfilComercialPayload? perfilComercial,
        IReadOnlyList<ReplaceTerceroDomicilioItem>? domicilios,
        IReadOnlyList<ReplaceTerceroContactoItem>? contactos,
        IReadOnlyList<ReplaceTerceroSucursalEntregaItem>? sucursalesEntrega,
        IReadOnlyList<ReplaceTerceroTransporteItem>? transportes,
        IReadOnlyList<ReplaceTerceroVentanaCobranzaItem>? ventanasCobranza,
        CancellationToken ct)
    {
        var perfilError = await UpsertPerfilComercialAsync(db, terceroId, userId, perfilComercial, ct);
        if (perfilError is not null)
            return perfilError;

        var domiciliosError = await ReplaceDomiciliosAsync(db, terceroId, domicilios, ct);
        if (domiciliosError is not null)
            return domiciliosError;

        var contactosError = await ReplaceContactosAsync(db, terceroId, userId, contactos, ct);
        if (contactosError is not null)
            return contactosError;

        var sucursalesError = await ReplaceSucursalesEntregaAsync(db, terceroId, userId, sucursalesEntrega, ct);
        if (sucursalesError is not null)
            return sucursalesError;

        var transportesError = await ReplaceTransportesAsync(db, terceroId, userId, transportes, ct);
        if (transportesError is not null)
            return transportesError;

        return await ReplaceVentanasCobranzaAsync(db, terceroId, userId, ventanasCobranza, ct);
    }

    private static async Task<string?> UpsertPerfilComercialAsync(
        IApplicationDbContext db,
        long terceroId,
        long? userId,
        TerceroPerfilComercialPayload? perfilComercial,
        CancellationToken ct)
    {
        if (perfilComercial is null)
            return null;

        if (perfilComercial.ZonaComercialId.HasValue)
        {
            var zonaExists = await db.ZonasComerciales
                .AsNoTracking()
                .AnyAsync(x => x.Id == perfilComercial.ZonaComercialId.Value && x.DeletedAt == null && x.Activo, ct);

            if (!zonaExists)
                return "La zona comercial indicada no existe o está inactiva.";
        }

        var riesgoTexto = string.IsNullOrWhiteSpace(perfilComercial.RiesgoCrediticio)
            ? RiesgoCrediticioComercial.Normal.ToString()
            : perfilComercial.RiesgoCrediticio.Trim();

        if (!Enum.TryParse<RiesgoCrediticioComercial>(riesgoTexto, true, out var riesgo))
            return "El riesgo crediticio indicado no es válido.";

        var perfil = await db.TercerosPerfilesComerciales
            .FirstOrDefaultAsync(x => x.TerceroId == terceroId && x.DeletedAt == null, ct);

        if (perfil is null)
        {
            perfil = TerceroPerfilComercial.Crear(terceroId, userId);
            db.TercerosPerfilesComerciales.Add(perfil);
        }

        try
        {
            perfil.Actualizar(
                perfilComercial.ZonaComercialId,
                perfilComercial.Rubro,
                perfilComercial.Subrubro,
                perfilComercial.Sector,
                perfilComercial.CondicionCobranza,
                riesgo,
                perfilComercial.SaldoMaximoVigente,
                perfilComercial.VigenciaSaldo,
                perfilComercial.VigenciaSaldoDesde,
                perfilComercial.VigenciaSaldoHasta,
                perfilComercial.CondicionVenta,
                perfilComercial.PlazoCobro,
                perfilComercial.FacturadorPorDefecto,
                perfilComercial.MinimoFacturaMipymes,
                perfilComercial.ObservacionComercial,
                userId);
        }
        catch (ArgumentException ex)
        {
            return ex.Message;
        }

        return null;
    }

    private static async Task<string?> ReplaceDomiciliosAsync(
        IApplicationDbContext db,
        long terceroId,
        IReadOnlyList<ReplaceTerceroDomicilioItem>? domicilios,
        CancellationToken ct)
    {
        if (domicilios is null)
            return null;

        var provinciaIds = domicilios
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
                return "Una o más provincias indicadas no existen.";
        }

        var localidadIds = domicilios
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
                return "Una o más localidades indicadas no existen.";
        }

        var existentes = await db.PersonasDomicilios
            .Where(x => x.PersonaId == terceroId)
            .OrderBy(x => x.Orden)
            .ToListAsync(ct);

        var defectoIndex = domicilios.Count == 0
            ? -1
            : domicilios.ToList().FindIndex(x => x.EsDefecto);

        var itemsNormalizados = domicilios
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
                return provinciaResult.Error;

            var existente = input.Item.Id.HasValue
                ? existentes.FirstOrDefault(x => x.Id == input.Item.Id.Value)
                : null;

            if (existente is null)
            {
                var nuevo = PersonaDomicilio.Crear(
                    terceroId,
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

        return null;
    }

    private static async Task<string?> ReplaceContactosAsync(
        IApplicationDbContext db,
        long terceroId,
        long? userId,
        IReadOnlyList<ReplaceTerceroContactoItem>? contactos,
        CancellationToken ct)
    {
        if (contactos is null)
            return null;

        var existentes = await db.TercerosContactos
            .Where(x => x.TerceroId == terceroId && x.DeletedAt == null)
            .OrderBy(x => x.Orden)
            .ToListAsync(ct);

        var principalIndex = contactos.Count == 0
            ? -1
            : contactos.ToList().FindIndex(x => x.Principal);

        var contactosNormalizados = contactos
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
            contacto.MarcarComoEliminado(userId);

        foreach (var contactoInput in contactosNormalizados)
        {
            var existente = contactoInput.Item.Id.HasValue
                ? existentes.FirstOrDefault(x => x.Id == contactoInput.Item.Id.Value)
                : null;

            try
            {
                if (existente is null)
                {
                    var nuevo = TerceroContacto.Crear(
                        terceroId,
                        contactoInput.Item.Nombre,
                        contactoInput.Item.Cargo,
                        contactoInput.Item.Email,
                        contactoInput.Item.Telefono,
                        contactoInput.Item.Sector,
                        contactoInput.Principal,
                        contactoInput.Orden,
                        userId);

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
                    userId);
            }
            catch (ArgumentException ex)
            {
                return ex.Message;
            }
        }

        return null;
    }

    private static async Task<string?> ReplaceSucursalesEntregaAsync(
        IApplicationDbContext db,
        long terceroId,
        long? userId,
        IReadOnlyList<ReplaceTerceroSucursalEntregaItem>? sucursalesEntrega,
        CancellationToken ct)
    {
        if (sucursalesEntrega is null)
            return null;

        var existentes = await db.TercerosSucursalesEntrega
            .Where(x => x.TerceroId == terceroId && x.DeletedAt == null)
            .OrderBy(x => x.Orden)
            .ToListAsync(ct);

        var principalIndex = sucursalesEntrega.Count == 0
            ? -1
            : sucursalesEntrega.ToList().FindIndex(x => x.Principal);

        var itemsNormalizados = sucursalesEntrega
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
            item.MarcarComoEliminada(userId);

        foreach (var input in itemsNormalizados)
        {
            var existente = input.Item.Id.HasValue
                ? existentes.FirstOrDefault(x => x.Id == input.Item.Id.Value)
                : null;

            try
            {
                if (existente is null)
                {
                    var nueva = TerceroSucursalEntrega.Crear(
                        terceroId,
                        input.Item.Descripcion,
                        input.Item.Direccion,
                        input.Item.Localidad,
                        input.Item.Responsable,
                        input.Item.Telefono,
                        input.Item.Horario,
                        input.Principal,
                        input.Orden,
                        userId);

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
                    userId);
            }
            catch (ArgumentException ex)
            {
                return ex.Message;
            }
        }

        return null;
    }

    private static async Task<string?> ReplaceTransportesAsync(
        IApplicationDbContext db,
        long terceroId,
        long? userId,
        IReadOnlyList<ReplaceTerceroTransporteItem>? transportes,
        CancellationToken ct)
    {
        if (transportes is null)
            return null;

        var transportistaIds = transportes
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
                return "Uno o más transportistas indicados no existen o están inactivos.";
        }

        var existentes = await db.TercerosTransportes
            .Where(x => x.TerceroId == terceroId && x.DeletedAt == null)
            .OrderBy(x => x.Orden)
            .ToListAsync(ct);

        var principalIndex = transportes.Count == 0
            ? -1
            : transportes.ToList().FindIndex(x => x.Principal);

        var itemsNormalizados = transportes
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
            item.MarcarComoEliminado(userId);

        foreach (var input in itemsNormalizados)
        {
            var existente = input.Item.Id.HasValue
                ? existentes.FirstOrDefault(x => x.Id == input.Item.Id.Value)
                : null;

            try
            {
                if (existente is null)
                {
                    var nuevo = TerceroTransporte.Crear(
                        terceroId,
                        input.Item.TransportistaId,
                        input.Item.Nombre,
                        input.Item.Servicio,
                        input.Item.Zona,
                        input.Item.Frecuencia,
                        input.Item.Observacion,
                        input.Item.Activo,
                        input.Principal,
                        input.Orden,
                        userId);

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
                    userId);
            }
            catch (ArgumentException ex)
            {
                return ex.Message;
            }
        }

        return null;
    }

    private static async Task<string?> ReplaceVentanasCobranzaAsync(
        IApplicationDbContext db,
        long terceroId,
        long? userId,
        IReadOnlyList<ReplaceTerceroVentanaCobranzaItem>? ventanasCobranza,
        CancellationToken ct)
    {
        if (ventanasCobranza is null)
            return null;

        var existentes = await db.TercerosVentanasCobranza
            .Where(x => x.TerceroId == terceroId && x.DeletedAt == null)
            .OrderBy(x => x.Orden)
            .ToListAsync(ct);

        var principalIndex = ventanasCobranza.Count == 0
            ? -1
            : ventanasCobranza.ToList().FindIndex(x => x.Principal);

        var itemsNormalizados = ventanasCobranza
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
            item.MarcarComoEliminada(userId);

        foreach (var input in itemsNormalizados)
        {
            var existente = input.Item.Id.HasValue
                ? existentes.FirstOrDefault(x => x.Id == input.Item.Id.Value)
                : null;

            try
            {
                if (existente is null)
                {
                    var nueva = TerceroVentanaCobranza.Crear(
                        terceroId,
                        input.Item.Dia,
                        input.Item.Franja,
                        input.Item.Canal,
                        input.Item.Responsable,
                        input.Principal,
                        input.Orden,
                        userId);

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
                    userId);
            }
            catch (ArgumentException ex)
            {
                return ex.Message;
            }
        }

        return null;
    }

    private static string? Normalize(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
