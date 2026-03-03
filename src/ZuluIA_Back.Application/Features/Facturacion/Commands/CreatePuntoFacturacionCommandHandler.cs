using MediatR;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Facturacion;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Facturacion.Commands;

public class CreatePuntoFacturacionCommandHandler(
    IPuntoFacturacionRepository repo,
    IUnitOfWork uow,
    ICurrentUserService currentUser)
    : IRequestHandler<CreatePuntoFacturacionCommand, Result<long>>
{
    public async Task<Result<long>> Handle(
        CreatePuntoFacturacionCommand request,
        CancellationToken ct)
    {
        if (await repo.ExisteNumeroAsync(request.SucursalId, request.Numero, null, ct))
            return Result.Failure<long>(
                $"Ya existe un punto de facturación con el número {request.Numero} " +
                $"en la sucursal {request.SucursalId}.");

        var punto = PuntoFacturacion.Crear(
            request.SucursalId,
            request.TipoId,
            request.Numero,
            request.Descripcion,
            currentUser.UserId);

        await repo.AddAsync(punto, ct);
        await uow.SaveChangesAsync(ct);

        return Result.Success(punto.Id);
    }
}