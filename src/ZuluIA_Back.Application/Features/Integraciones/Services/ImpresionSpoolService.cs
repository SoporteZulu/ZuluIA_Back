using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Impresion.Enums;
using ZuluIA_Back.Application.Features.Impresion.Services;
using ZuluIA_Back.Domain.Entities.Integraciones;
using ZuluIA_Back.Domain.Enums;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Integraciones.Services;

public class ImpresionSpoolService(
    IApplicationDbContext db,
    IRepository<ImpresionSpoolTrabajo> repo,
    ImpresionFiscalService impresionFiscalService,
    OperacionesBatchSettingsService settingsService,
    ICurrentUserService currentUser)
{
    public async Task<ImpresionSpoolTrabajo> EncolarFiscalAsync(long comprobanteId, MarcaImpresoraFiscal marca, CancellationToken ct)
    {
        var existente = await db.ImpresionSpoolTrabajos.FirstOrDefaultAsync(
            x => x.DeletedAt == null
                && x.ComprobanteId == comprobanteId
                && x.TipoTrabajo == "FISCAL"
                && x.Destino == marca.ToString().ToUpperInvariant()
                && x.Estado != EstadoImpresionSpool.Completado,
            ct);

        if (existente is not null)
            return existente;

        var trabajo = ImpresionSpoolTrabajo.Encolar(comprobanteId, "FISCAL", marca.ToString(), currentUser.UserId);
        await repo.AddAsync(trabajo, ct);
        return trabajo;
    }

    public async Task<int> ProcesarPendientesAsync(CancellationToken ct)
    {
        var settings = await settingsService.ResolveAsync(ct);
        if (!settings.SpoolHabilitado)
            return 0;

        var ahora = DateTimeOffset.UtcNow;
        var pendientes = await db.ImpresionSpoolTrabajos
            .Where(x => x.DeletedAt == null
                && ((x.Estado == EstadoImpresionSpool.Pendiente && (!x.ProximoIntento.HasValue || x.ProximoIntento <= ahora))
                    || (x.Estado == EstadoImpresionSpool.Error && x.ProximoIntento.HasValue && x.ProximoIntento <= ahora)))
            .OrderBy(x => x.CreatedAt)
            .Take(settings.SpoolLote)
            .ToListAsync(ct);

        var procesados = 0;

        foreach (var trabajo in pendientes)
        {
            trabajo.MarcarProcesando(currentUser.UserId);
            repo.Update(trabajo);
            await db.SaveChangesAsync(ct);

            try
            {
                var marca = Enum.Parse<MarcaImpresoraFiscal>(trabajo.Destino, true);
                var result = await impresionFiscalService.ImprimirComprobanteAsync(trabajo.ComprobanteId, marca, ct);
                trabajo.Completar(result.PayloadFiscal, currentUser.UserId);
            }
            catch (Exception ex)
            {
                var mensajeError = ClassifyError(ex);
                var proximoIntento = CalculateNextRetry(ahora, trabajo.Intentos, settings);
                trabajo.Fallar(mensajeError, proximoIntento, currentUser.UserId);
            }

            repo.Update(trabajo);
            await db.SaveChangesAsync(ct);
            procesados++;
        }

        return procesados;
    }

    public async Task ReencolarAsync(long id, CancellationToken ct)
    {
        var trabajo = await db.ImpresionSpoolTrabajos.FirstOrDefaultAsync(x => x.Id == id && x.DeletedAt == null, ct)
            ?? throw new InvalidOperationException($"No se encontró el trabajo de spool ID {id}.");

        trabajo.Reencolar(DateTimeOffset.UtcNow, currentUser.UserId);
        repo.Update(trabajo);
    }

    private static DateTimeOffset? CalculateNextRetry(DateTimeOffset ahora, int intentos, OperacionesBatchSettings settings)
    {
        if (intentos >= settings.SpoolMaxIntentos)
            return null;

        var factor = Math.Max(1, settings.SpoolBackoffFactor);
        var retryMinutes = settings.SpoolReintentoMinutos * (int)Math.Pow(factor, Math.Max(0, intentos - 1));
        retryMinutes = Math.Min(Math.Max(settings.SpoolReintentoMinutos, retryMinutes), Math.Max(settings.SpoolReintentoMinutos, settings.SpoolMaxRetryMinutes));
        return ahora.AddMinutes(retryMinutes);
    }

    private static string ClassifyError(Exception ex)
    {
        var message = ex.Message.Trim();
        var upper = message.ToUpperInvariant();
        if (upper.Contains("ADAPTADOR") || upper.Contains("IMPRESORA"))
            return $"IMPRESORA_NO_DISPONIBLE | {message}";
        if (upper.Contains("COMPROBANTE") && upper.Contains("NO SE ENCONTRÓ"))
            return $"COMPROBANTE_NO_ENCONTRADO | {message}";
        if (ex is OperationCanceledException or TimeoutException)
            return $"TIMEOUT_IMPRESION | {message}";

        return $"ERROR_IMPRESION | {message}";
    }
}
