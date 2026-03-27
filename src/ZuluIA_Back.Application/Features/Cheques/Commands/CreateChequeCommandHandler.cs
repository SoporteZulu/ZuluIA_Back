using MediatR;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Cheques.Services;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Finanzas;
using ZuluIA_Back.Domain.Enums;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Cheques.Commands;

public class CreateChequeCommandHandler(
    IChequeRepository repo,
    ChequeAuditoriaService auditoriaService,
    IUnitOfWork uow,
    ICurrentUserService currentUser)
    : IRequestHandler<CreateChequeCommand, Result<long>>
{
    public async Task<Result<long>> Handle(CreateChequeCommand request, CancellationToken ct)
    {
        var cheque = Cheque.Crear(
            request.CajaId,
            request.TerceroId,
            request.NroCheque,
            request.Banco,
            request.FechaEmision,
            request.FechaVencimiento,
            request.Importe,
            request.MonedaId,
            request.Observacion,
            currentUser.UserId);

        await repo.AddAsync(cheque, ct);
        await uow.SaveChangesAsync(ct);

        await auditoriaService.RegistrarAsync(
            cheque,
            TipoOperacionCheque.Alta,
            null,
            request.FechaEmision,
            null,
            request.TerceroId,
            request.Observacion,
            currentUser.UserId,
            ct);

        await uow.SaveChangesAsync(ct);

        return Result.Success(cheque.Id);
    }
}