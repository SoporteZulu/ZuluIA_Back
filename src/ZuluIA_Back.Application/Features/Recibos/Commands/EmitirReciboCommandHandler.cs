using MediatR;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Finanzas;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Recibos.Commands;

public class EmitirReciboCommandHandler(
    IReciboRepository reciboRepo,
    IUnitOfWork uow,
    ICurrentUserService currentUser)
    : IRequestHandler<EmitirReciboCommand, Result<long>>
{
    public async Task<Result<long>> Handle(EmitirReciboCommand request, CancellationToken ct)
    {
        if (!request.Items.Any())
            return Result.Failure<long>("El recibo debe tener al menos un concepto.");

        var numero = await reciboRepo.GetUltimoNumeroAsync(request.SucursalId, request.Serie, ct) + 1;

        var recibo = Recibo.Crear(
            request.SucursalId,
            request.TerceroId,
            request.Fecha,
            request.Serie,
            numero,
            request.Observacion,
            request.CobroId,
            null,
            null,
            null,
            currentUser.UserId,
            null,
            null,
            null,
            null,
            currentUser.UserId);

        foreach (var item in request.Items)
            recibo.AgregarItem(ReciboItem.Crear(0, item.Descripcion, item.Importe));

        await reciboRepo.AddAsync(recibo, ct);
        await uow.SaveChangesAsync(ct);

        return Result.Success(recibo.Id);
    }
}
