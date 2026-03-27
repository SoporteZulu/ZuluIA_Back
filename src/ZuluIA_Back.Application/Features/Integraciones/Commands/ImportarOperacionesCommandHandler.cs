using MediatR;
using ZuluIA_Back.Application.Features.Integraciones.Services;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Integraciones.Commands;

public class ImportarOperacionesCommandHandler(
    IUnitOfWork uow,
    IntegracionProcesoService procesoService)
    : IRequestHandler<ImportarOperacionesCommand, Result<long>>
{
    public async Task<Result<long>> Handle(ImportarOperacionesCommand request, CancellationToken ct)
    {
        var job = await procesoService.CrearJobAsync(Domain.Enums.TipoProcesoIntegracion.ImportacionOperativa, request.Nombre, request.Operaciones.Count, request.Observacion, ct);
        await uow.SaveChangesAsync(ct);

        foreach (var operacion in request.Operaciones)
        {
            if (operacion.Exitoso)
            {
                procesoService.RegistrarExito(job);
                await procesoService.RegistrarLogAsync(job.Id, Domain.Enums.NivelLogIntegracion.Informacion, $"Operación {operacion.Tipo} importada correctamente.", operacion.Referencia, operacion.Payload, ct);
                continue;
            }

            procesoService.RegistrarError(job, operacion.Referencia);
            await procesoService.RegistrarLogAsync(job.Id, Domain.Enums.NivelLogIntegracion.Error, $"Operación {operacion.Tipo} con error.", operacion.Referencia, operacion.Payload, ct);
        }

        procesoService.Finalizar(job, request.Observacion);
        await uow.SaveChangesAsync(ct);
        return Result.Success(job.Id);
    }
}
