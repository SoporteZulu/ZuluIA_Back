using MediatR;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Finanzas;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Cajas.Commands;

public class CreateCajaCommandHandler(
    ICajaRepository repo,
    IUnitOfWork uow,
    ICurrentUserService currentUser)
    : IRequestHandler<CreateCajaCommand, Result<long>>
{
    public async Task<Result<long>> Handle(CreateCajaCommand request, CancellationToken ct)
    {
        var caja = CajaCuentaBancaria.Crear(
            request.SucursalId,
            request.TipoId,
            request.Descripcion,
            request.MonedaId,
            request.EsCaja,
            request.UsuarioId,
            currentUser.UserId);

        caja.Actualizar(
            request.Descripcion,
            request.TipoId,
            request.MonedaId,
            request.Banco,
            request.NroCuenta,
            request.Cbu,
            request.UsuarioId,
            request.EsCaja,
            currentUser.UserId);

        await repo.AddAsync(caja, ct);
        await uow.SaveChangesAsync(ct);

        return Result.Success(caja.Id);
    }
}