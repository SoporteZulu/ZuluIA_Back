using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Terceros.DTOs;

namespace ZuluIA_Back.Application.Features.Terceros.Queries;

internal static class TerceroMedioContactoReadModelLoader
{
    private const string RelationName = "PER_MEDIOCONTACTO";
    private const string UndefinedTableSqlState = "42P01";
    private const string TipoOtroCodigo = "OTRO";
    private const string TipoOtroDescripcion = "Otro";

    public static async Task<IReadOnlyList<TerceroMedioContactoDto>> LoadAsync(
        IApplicationDbContext db,
        long terceroId,
        ILogger logger,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(db);
        ArgumentNullException.ThrowIfNull(logger);

        List<Domain.Entities.Terceros.MedioContacto> medios;

        try
        {
            medios = await db.MediosContacto
                .AsNoTracking()
                .Where(x => x.PersonaId == terceroId)
                .OrderByDescending(x => x.EsDefecto)
                .ThenBy(x => x.Orden)
                .ThenBy(x => x.Id)
                .ToListAsync(ct);
        }
        catch (DbException ex) when (IsMissingRelation(ex))
        {
            logger.LogWarning(
                ex,
                "No se encontró la relación {RelationName} al cargar medios de contacto para el tercero {TerceroId}. Se devuelve una lista vacía hasta aplicar el upgrade local.",
                RelationName,
                terceroId);

            return [];
        }

        return medios.Select(x =>
        {
            var tipoInferido = InferirTipo(x.Valor, x.TipoMedioContactoId);

            return new TerceroMedioContactoDto
            {
                Id = x.Id,
                TerceroId = x.PersonaId,
                TipoMedioContactoId = x.TipoMedioContactoId,
                Valor = x.Valor,
                TipoInferidoCodigo = tipoInferido.Codigo,
                TipoInferidoDescripcion = tipoInferido.Descripcion,
                Observacion = x.Observacion,
                Orden = x.Orden,
                EsDefecto = x.EsDefecto
            };
        }).ToList();
    }

    private static bool IsMissingRelation(DbException exception)
    {
        var sqlState = exception.GetType().GetProperty("SqlState")?.GetValue(exception) as string;
        if (string.Equals(sqlState, UndefinedTableSqlState, StringComparison.Ordinal)
            && exception.Message.Contains(RelationName, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return exception.Message.Contains(UndefinedTableSqlState, StringComparison.Ordinal)
            && exception.Message.Contains(RelationName, StringComparison.OrdinalIgnoreCase);
    }

    private static (string Codigo, string Descripcion) InferirTipo(string? valor, long? tipoMedioContactoId)
    {
        if (string.IsNullOrWhiteSpace(valor))
            return FallbackByTipoId(tipoMedioContactoId);

        var normalized = valor.Trim();
        var lower = normalized.ToLowerInvariant();

        if (normalized.Contains('@'))
            return ("EMAIL", "Correo electrónico");

        if (IsWebCandidate(normalized))
            return ("WEB", "Sitio web");

        if (lower.Contains("whatsapp") || normalized.Contains("+54 9", StringComparison.OrdinalIgnoreCase))
            return ("CELULAR", "Celular");

        var digitsCount = normalized.Count(char.IsDigit);
        if (digitsCount >= 6)
            return ("TELEFONO", "Teléfono");

        return FallbackByTipoId(tipoMedioContactoId);
    }

    private static bool IsWebCandidate(string value)
        => value.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
           || value.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
           || value.StartsWith("www.", StringComparison.OrdinalIgnoreCase)
           || Uri.TryCreate(value, UriKind.Absolute, out _);

    private static (string Codigo, string Descripcion) FallbackByTipoId(long? tipoMedioContactoId)
        => tipoMedioContactoId.HasValue
            ? ($"TIPO_{tipoMedioContactoId.Value}", $"Tipo {tipoMedioContactoId.Value}")
            : (TipoOtroCodigo, TipoOtroDescripcion);
}
