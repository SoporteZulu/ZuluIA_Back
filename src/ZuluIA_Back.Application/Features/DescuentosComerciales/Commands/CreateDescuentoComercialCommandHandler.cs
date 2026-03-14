using MediatR;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Ventas;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.DescuentosComerciales.Commands;

public class CreateDescuentoComercialCommandHandler(
    IApplicationDbContext db,
    IUnitOfWork uow,
    ICurrentUserService currentUser)
    : IRequestHandler<CreateDescuentoComercialCommand, Result<long>>
{
    public async Task<Result<long>> Handle(
        CreateDescuentoComercialCommand request,
        CancellationToken ct)
    {
        try
        {
            var descuento = DescuentoComercial.Crear(
                request.TerceroId,
                request.ItemId,
                request.FechaDesde,
                request.FechaHasta,
                request.Porcentaje,
                currentUser.UserId);

            db.DescuentosComerciales.Add(descuento);
            await uow.SaveChangesAsync(ct);
            return Result.Success(descuento.Id);
        }
        catch (ArgumentOutOfRangeException ex)
        {
            return Result.Failure<long>(ex.Message);
        }
        catch (ArgumentException ex)
        {
            return Result.Failure<long>(ex.Message);
        }
    }
}
