using MediatR;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Facturacion.Services;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Facturacion.Commands;

public class CrearOrdenCargaCommandHandler(
    IApplicationDbContext db,
    CartaPorteWorkflowService workflowService,
    IUnitOfWork uow)
    : IRequestHandler<CrearOrdenCargaCommand, Result<long>>
{
    public async Task<Result<long>> Handle(CrearOrdenCargaCommand request, CancellationToken ct)
    {
        try
        {
            var orden = await workflowService.CrearOrdenCargaAsync(
                request.CartaPorteId,
                request.TransportistaId,
                request.FechaCarga,
                request.Origen,
                request.Destino,
                request.Patente,
                request.Observacion,
                ct);

            await uow.SaveChangesAsync(ct);

            string? cuitTransportista = null;
            if (request.TransportistaId.HasValue)
            {
                cuitTransportista = await db.Transportistas.AsNoTracking()
                    .Where(x => x.Id == request.TransportistaId.Value)
                    .Select(x => x.NroCuitTransportista)
                    .FirstOrDefaultAsync(ct);
            }

            await workflowService.AsignarOrdenCargaAsync(
                request.CartaPorteId,
                orden.Id,
                request.TransportistaId,
                cuitTransportista,
                request.Observacion,
                request.FechaCarga,
                ct);

            await uow.SaveChangesAsync(ct);
            return Result.Success(orden.Id);
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure<long>(ex.Message);
        }
    }
}
