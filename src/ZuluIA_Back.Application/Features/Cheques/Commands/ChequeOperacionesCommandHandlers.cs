using MediatR;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Cheques.Services;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Enums;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Cheques.Commands;

public class EndosarChequeCommandHandler(
    IChequeRepository repo,
    ChequeAuditoriaService auditoriaService,
    IUnitOfWork uow,
    ICurrentUserService currentUser)
    : IRequestHandler<EndosarChequeCommand, Result>
{
    public async Task<Result> Handle(EndosarChequeCommand request, CancellationToken ct)
    {
        var cheque = await repo.GetByIdAsync(request.ChequeId, ct);
        if (cheque is null)
            return Result.Failure("Cheque no encontrado.");

        try
        {
            cheque.Endosar(request.NuevoTerceroId, request.Observacion, currentUser.UserId);
            await uow.SaveChangesAsync(ct);

            await auditoriaService.RegistrarAsync(
                cheque,
                TipoOperacionCheque.Endoso,
                null,
                DateOnly.FromDateTime(DateTime.Today),
                null,
                request.NuevoTerceroId,
                request.Observacion,
                currentUser.UserId,
                ct);

            await uow.SaveChangesAsync(ct);
            return Result.Success();
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(ex.Message);
        }
    }
}

public class AnularChequePropioCommandHandler(
    IChequeRepository repo,
    ChequeAuditoriaService auditoriaService,
    IUnitOfWork uow,
    ICurrentUserService currentUser)
    : IRequestHandler<AnularChequePropioCommand, Result>
{
    public async Task<Result> Handle(AnularChequePropioCommand request, CancellationToken ct)
    {
        var cheque = await repo.GetByIdAsync(request.ChequeId, ct);
        if (cheque is null)
            return Result.Failure("Cheque no encontrado.");

        try
        {
            cheque.Anular(request.Motivo, currentUser.UserId);
            await uow.SaveChangesAsync(ct);

            await auditoriaService.RegistrarAsync(
                cheque,
                TipoOperacionCheque.Anulacion,
                null,
                DateOnly.FromDateTime(DateTime.Today),
                null,
                null,
                $"Anulado: {request.Motivo}",
                currentUser.UserId,
                ct);

            await uow.SaveChangesAsync(ct);
            return Result.Success();
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(ex.Message);
        }
    }
}

public class ActualizarChequeCommandHandler(
    IChequeRepository repo,
    IUnitOfWork uow,
    ICurrentUserService currentUser)
    : IRequestHandler<ActualizarChequeCommand, Result>
{
    public async Task<Result> Handle(ActualizarChequeCommand request, CancellationToken ct)
    {
        var cheque = await repo.GetByIdAsync(request.ChequeId, ct);
        if (cheque is null)
            return Result.Failure("Cheque no encontrado.");

        try
        {
            cheque.ActualizarDatos(
                request.Titular,
                request.FechaEmision,
                request.FechaVencimiento,
                request.CodigoSucursalBancaria,
                request.CodigoPostal,
                currentUser.UserId);

            await uow.SaveChangesAsync(ct);
            return Result.Success();
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(ex.Message);
        }
    }
}
