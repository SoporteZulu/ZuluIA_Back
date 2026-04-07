using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Diagnosticos.Commands;
using ZuluIA_Back.Domain.Entities.Diagnosticos;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Application.Features.Diagnosticos.Services;

public class DiagnosticoPlanTrabajoService(
    IApplicationDbContext db,
    ICurrentUserService currentUser)
{
    public async Task<decimal> EvaluarAsync(long planillaId, IReadOnlyList<RespuestaDiagnosticaInput> respuestas, string? observacion, CancellationToken ct = default)
    {
        var planilla = await db.PlanillasDiagnosticas
            .Include(x => x.Respuestas)
            .FirstOrDefaultAsync(x => x.Id == planillaId, ct)
            ?? throw new InvalidOperationException($"No se encontró la planilla diagnóstica con ID {planillaId}.");

        var plantillaVariables = await db.PlantillasDiagnosticasVariables.AsNoTracking()
            .Where(x => x.PlantillaId == planilla.PlantillaId)
            .Select(x => x.VariableId)
            .ToListAsync(ct);

        var variables = await db.VariablesDiagnosticas.AsNoTracking()
            .Where(x => plantillaVariables.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, ct);

        var opciones = await db.VariablesDiagnosticasOpciones.AsNoTracking()
            .Where(x => plantillaVariables.Contains(x.VariableId))
            .ToListAsync(ct);

        foreach (var variable in variables.Values.Where(x => x.Requerida))
        {
            if (!respuestas.Any(x => x.VariableId == variable.Id))
                throw new InvalidOperationException($"La variable requerida '{variable.Codigo}' no posee respuesta.");
        }

        decimal totalPeso = 0m;
        decimal totalPonderado = 0m;

        foreach (var respuestaInput in respuestas)
        {
            if (!variables.TryGetValue(respuestaInput.VariableId, out var variable))
                throw new InvalidOperationException($"La variable ID {respuestaInput.VariableId} no pertenece a la plantilla.");

            decimal puntaje = variable.Tipo switch
            {
                TipoVariableDiagnostica.Opcion => opciones.FirstOrDefault(x => x.Id == respuestaInput.OpcionId && x.VariableId == variable.Id)?.ValorNumerico
                    ?? throw new InvalidOperationException($"La opción seleccionada para la variable '{variable.Codigo}' no es válida."),
                TipoVariableDiagnostica.Numero => respuestaInput.ValorNumerico ?? 0m,
                _ => 0m
            };

            totalPeso += variable.Peso;
            totalPonderado += puntaje * variable.Peso;

            planilla.RegistrarRespuesta(PlanillaDiagnosticaRespuesta.Registrar(
                planilla.Id,
                variable.Id,
                respuestaInput.OpcionId,
                respuestaInput.ValorTexto,
                respuestaInput.ValorNumerico,
                puntaje));
        }

        var resultado = totalPeso == 0 ? 0 : decimal.Round(totalPonderado / totalPeso, 2, MidpointRounding.AwayFromZero);
        planilla.Evaluar(resultado, observacion, currentUser.UserId);
        return resultado;
    }
}
