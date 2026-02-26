using MediatR;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Interfaces;
using ZuluIA_Back.Domain.ValueObjects;

namespace ZuluIA_Back.Application.Features.Terceros.Commands;

public class UpdateTerceroCommandHandler(
    ITerceroRepository repo,
    IUnitOfWork uow,
    ICurrentUserService currentUser)
    : IRequestHandler<UpdateTerceroCommand, Result>
{
    public async Task<Result> Handle(UpdateTerceroCommand request, CancellationToken ct)
    {
        var tercero = await repo.GetByIdAsync(request.Id, ct);

        if (tercero is null)
            return Result.Failure($"No se encontró el tercero con ID {request.Id}.");

        tercero.Actualizar(
            request.RazonSocial,
            request.NombreFantasia,
            request.Telefono,
            request.Celular,
            request.Email,
            request.Web,
            new Domicilio(
                request.Calle,
                request.Nro,
                null,
                null,
                request.CodigoPostal,
                request.LocalidadId,
                request.BarrioId),
            request.LimiteCredito,
            currentUser.UserId);

        if (request.CategoriaId.HasValue)
            tercero.SetCategoria(request.CategoriaId);

        if (request.MonedaId.HasValue)
            tercero.SetMoneda(request.MonedaId.Value);

        repo.Update(tercero);
        await uow.SaveChangesAsync(ct);

        return Result.Success();
    }
}