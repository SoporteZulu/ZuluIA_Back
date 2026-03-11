using MediatR;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Sucursales;
using ZuluIA_Back.Domain.Interfaces;
using ZuluIA_Back.Domain.ValueObjects;

namespace ZuluIA_Back.Application.Features.Sucursales.Commands;

public class CreateSucursalCommandHandler(
    ISucursalRepository repo,
    IUnitOfWork uow,
    ICurrentUserService currentUser)
    : IRequestHandler<CreateSucursalCommand, Result<long>>
{
    public async Task<Result<long>> Handle(CreateSucursalCommand request, CancellationToken ct)
    {
        if (await repo.ExisteCuitAsync(request.Cuit, null, ct))
            return Result.Failure<long>($"Ya existe una sucursal con el CUIT '{request.Cuit}'.");

        var sucursal = Sucursal.Crear(
            request.RazonSocial,
            request.Cuit,
            request.CondicionIvaId,
            request.MonedaId,
            request.PaisId,
            request.CasaMatriz,
            currentUser.UserId);

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

        await repo.AddAsync(sucursal, ct);
        await uow.SaveChangesAsync(ct);

        return Result.Success(sucursal.Id);
    }
}