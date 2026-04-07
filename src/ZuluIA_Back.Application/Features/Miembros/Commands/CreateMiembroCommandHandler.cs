using MediatR;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Terceros;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Miembros.Commands;

public class CreateMiembroCommandHandler(
    ITerceroRepository repo,
    IUnitOfWork uow,
    ICurrentUserService currentUser)
    : IRequestHandler<CreateMiembroCommand, Result<long>>
{
    public async Task<Result<long>> Handle(CreateMiembroCommand request, CancellationToken ct)
    {
        var legajo = request.Legajo.Trim().ToUpperInvariant();

        if (await repo.ExisteLegajoAsync(legajo, null, ct))
            return Result.Failure<long>("Ya existe un miembro con ese legajo.");

        var entity = Tercero.Crear(
            legajo,
            request.Nombre,
            request.TipoDocumentoId,
            request.NroDocumento,
            request.CondicionIvaId,
            esCliente: true,
            esProveedor: false,
            esEmpleado: false,
            request.SucursalId,
            currentUser.UserId);

        await repo.AddAsync(entity, ct);
        await uow.SaveChangesAsync(ct);

        return Result.Success(entity.Id);
    }
}