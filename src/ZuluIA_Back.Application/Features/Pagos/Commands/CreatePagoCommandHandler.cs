using MediatR;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Finanzas;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Pagos.Commands;

public class CreatePagoCommandHandler(
    IRepository<Pago> repo,
    IUnitOfWork uow,
    ICurrentUserService currentUser)
    : IRequestHandler<CreatePagoCommand, Result<long>>
{
    public async Task<Result<long>> Handle(CreatePagoCommand request, CancellationToken ct)
    {
        if (!request.Medios.Any())
            return Result.Failure<long>("El pago debe tener al menos un medio de pago.");

        var pago = Pago.Crear(
            request.SucursalId,
            request.TerceroId,
            request.Fecha,
            request.MonedaId,
            request.Cotizacion,
            request.Observacion,
            currentUser.UserId);

        foreach (var medioDto in request.Medios)
        {
            var medio = PagoMedio.Crear(
                0,
                medioDto.CajaId,
                medioDto.FormaPagoId,
                medioDto.Importe,
                medioDto.MonedaId,
                medioDto.Cotizacion,
                medioDto.ChequeId);

            pago.AgregarMedio(medio);
        }

        await repo.AddAsync(pago, ct);
        await uow.SaveChangesAsync(ct);

        return Result.Success(pago.Id);
    }
}