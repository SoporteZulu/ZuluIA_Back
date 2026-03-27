using MediatR;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Cheques.Services;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Enums;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Cheques.Commands;

public class CambiarEstadoChequeCommandHandler(
    IChequeRepository repo,
    ChequeAuditoriaService auditoriaService,
    IUnitOfWork uow,
    ICurrentUserService currentUser)
    : IRequestHandler<CambiarEstadoChequeCommand, Result>
{
    public async Task<Result> Handle(CambiarEstadoChequeCommand request, CancellationToken ct)
    {
        var cheque = await repo.GetByIdAsync(request.Id, ct);
        if (cheque is null)
            return Result.Failure($"No se encontró el cheque con ID {request.Id}.");

        var estadoAnterior = cheque.Estado;

        switch (request.Accion)
        {
            case AccionCheque.Depositar:
                if (!request.Fecha.HasValue)
                    return Result.Failure("La fecha de depósito es obligatoria.");
                cheque.Depositar(request.Fecha.Value, request.FechaAcreditacion, currentUser.UserId);
                await auditoriaService.RegistrarAsync(
                    cheque,
                    TipoOperacionCheque.Deposito,
                    estadoAnterior,
                    request.Fecha.Value,
                    request.FechaAcreditacion,
                    request.TerceroId,
                    request.Observacion,
                    currentUser.UserId,
                    ct);
                break;

            case AccionCheque.Acreditar:
                if (!request.Fecha.HasValue)
                    return Result.Failure("La fecha de acreditación es obligatoria.");
                cheque.Acreditar(request.Fecha.Value, currentUser.UserId);
                await auditoriaService.RegistrarAsync(
                    cheque,
                    TipoOperacionCheque.Acreditacion,
                    estadoAnterior,
                    request.Fecha.Value,
                    request.Fecha.Value,
                    request.TerceroId,
                    request.Observacion,
                    currentUser.UserId,
                    ct);
                break;

            case AccionCheque.Rechazar:
                cheque.Rechazar(request.Observacion, currentUser.UserId);
                await auditoriaService.RegistrarAsync(
                    cheque,
                    TipoOperacionCheque.Rechazo,
                    estadoAnterior,
                    request.Fecha ?? DateOnly.FromDateTime(DateTime.Today),
                    null,
                    request.TerceroId,
                    request.Observacion,
                    currentUser.UserId,
                    ct);
                break;

            case AccionCheque.Entregar:
                cheque.Entregar(request.TerceroId, request.Observacion, currentUser.UserId);
                await auditoriaService.RegistrarAsync(
                    cheque,
                    TipoOperacionCheque.Entrega,
                    estadoAnterior,
                    request.Fecha ?? DateOnly.FromDateTime(DateTime.Today),
                    null,
                    request.TerceroId,
                    request.Observacion,
                    currentUser.UserId,
                    ct);
                break;

            default:
                return Result.Failure($"Acción no reconocida: {request.Accion}.");
        }

        repo.Update(cheque);
        await uow.SaveChangesAsync(ct);

        return Result.Success();
    }
}