using MediatR;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Finanzas;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Cobros.Commands;

public class CreateCobroCommandHandler(
    IRepository<Cobro> repo,
    IUnitOfWork uow,
    ICurrentUserService currentUser)
    : IRequestHandler<CreateCobroCommand, Result<long>>
{
    public async Task<Result<long>> Handle(CreateCobroCommand request, CancellationToken ct)
    {
        if (!request.Medios.Any())
            return Result.Failure<long>("El cobro debe tener al menos un medio de pago.");

        var cobro = Cobro.Crear(
            request.SucursalId,
            request.TerceroId,
            request.Fecha,
            request.MonedaId,
            request.Cotizacion,
            request.Observacion,
            currentUser.UserId);

        foreach (var medioDto in request.Medios)
        {
            var medio = CobroMedio.Crear(
                0,
                medioDto.CajaId,
                medioDto.FormaPagoId,
                medioDto.Importe,
                medioDto.MonedaId,
                medioDto.Cotizacion,
                medioDto.ChequeId);

            cobro.AgregarMedio(medio);
        }

        await repo.AddAsync(cobro, ct);
        await uow.SaveChangesAsync(ct);

        return Result.Success(cobro.Id);
    }
}