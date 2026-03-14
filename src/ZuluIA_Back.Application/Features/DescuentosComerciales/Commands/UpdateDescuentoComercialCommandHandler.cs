using MediatR;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Ventas;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.DescuentosComerciales.Commands;

public class UpdateDescuentoComercialCommandHandler(
    IRepository<DescuentoComercial> repo,
    IUnitOfWork uow,
    ICurrentUserService currentUser)
    : IRequestHandler<UpdateDescuentoComercialCommand, Result>
{
    public async Task<Result> Handle(UpdateDescuentoComercialCommand request, CancellationToken ct)
    {
        var descuento = await repo.GetByIdAsync(request.Id, ct);
        if (descuento is null)
            return Result.Failure($"No se encontró el descuento comercial con ID {request.Id}.");

        try
        {
            descuento.Actualizar(request.FechaDesde, request.FechaHasta, request.Porcentaje, currentUser.UserId);
            repo.Update(descuento);
            await uow.SaveChangesAsync(ct);
            return Result.Success();
        }
        catch (Exception ex) when (ex is ArgumentException or ArgumentOutOfRangeException)
        {
            return Result.Failure(ex.Message);
        }
    }
}
