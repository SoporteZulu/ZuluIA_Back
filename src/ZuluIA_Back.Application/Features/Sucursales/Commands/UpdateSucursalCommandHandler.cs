using MediatR;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Interfaces;
using ZuluIA_Back.Domain.ValueObjects;

namespace ZuluIA_Back.Application.Features.Sucursales.Commands;

public class UpdateSucursalCommandHandler(
    ISucursalRepository repo,
    IUnitOfWork uow,
    ICurrentUserService currentUser)
    : IRequestHandler<UpdateSucursalCommand, Result>
{
    public async Task<Result> Handle(UpdateSucursalCommand request, CancellationToken ct)
    {
        var sucursal = await repo.GetByIdAsync(request.Id, ct);
        if (sucursal is null)
            return Result.Failure($"No se encontró la sucursal con ID {request.Id}.");

        if (await repo.ExisteCuitAsync(request.Cuit, request.Id, ct))
            return Result.Failure($"Ya existe otra sucursal con el CUIT '{request.Cuit}'.");

        sucursal.Actualizar(
            request.RazonSocial,
            request.NombreFantasia,
            request.Cuit,
            request.NroIngresosBrutos,
            request.CondicionIvaId,
            request.MonedaId,
            request.PaisId,
            new Domicilio(request.Calle, request.Nro, request.Piso, request.Dpto,
                          request.CodigoPostal, request.LocalidadId, request.BarrioId),
            request.Telefono,
            request.Email,
            request.Web,
            request.Cbu,
            request.AliasCbu,
            request.Cai,
            request.PuertoAfip,
            request.CasaMatriz,
            currentUser.UserId);

        repo.Update(sucursal);
        await uow.SaveChangesAsync(ct);

        return Result.Success();
    }
}