using MediatR;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Finanzas.Services;
using ZuluIA_Back.Application.Features.Tesoreria.Services;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Finanzas;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Finanzas.Commands;

public class RegistrarPagoCommandHandler(
    IPagoRepository pagoRepo,
    PagoWorkflowService pagoWorkflowService,
    IUnitOfWork uow,
    ICurrentUserService currentUser)
    : IRequestHandler<RegistrarPagoCommand, Result<long>>
{
    public async Task<Result<long>> Handle(
        RegistrarPagoCommand request,
        CancellationToken ct)
    {
        if (!request.Medios.Any())
            return Result.Failure<long>(
                "El pago debe tener al menos un medio de pago.");

        // 1. Crear pago
        var pago = Pago.Crear(
            request.SucursalId,
            request.TerceroId,
            request.Fecha,
            request.MonedaId,
            request.Cotizacion,
            request.Observacion,
            currentUser.UserId);

        // 2. Agregar medios
        foreach (var medio in request.Medios)
        {
            pago.AgregarMedio(PagoMedio.Crear(
                0,
                medio.CajaId,
                medio.FormaPagoId,
                medio.ChequeId,
                medio.Importe,
                medio.MonedaId,
                medio.Cotizacion));
        }

        await pagoRepo.AddAsync(pago, ct);
        await uow.SaveChangesAsync(ct);

        var workflowResult = await pagoWorkflowService.ApplyAsync(pago, request, currentUser.UserId, ct);
        if (workflowResult is not null)
            return workflowResult;

        await uow.SaveChangesAsync(ct);
        return Result.Success(pago.Id);
    }
}