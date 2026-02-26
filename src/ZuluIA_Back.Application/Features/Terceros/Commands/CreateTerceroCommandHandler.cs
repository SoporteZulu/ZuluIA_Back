using MediatR;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Terceros;
using ZuluIA_Back.Domain.Interfaces;
using ZuluIA_Back.Domain.ValueObjects;

namespace ZuluIA_Back.Application.Features.Terceros.Commands;

public class CreateTerceroCommandHandler(
    ITerceroRepository repo,
    IUnitOfWork uow,
    ICurrentUserService currentUser)
    : IRequestHandler<CreateTerceroCommand, Result<long>>
{
    public async Task<Result<long>> Handle(CreateTerceroCommand request, CancellationToken ct)
    {
        if (await repo.ExisteLegajoAsync(request.Legajo, null, ct))
            return Result.Failure<long>($"Ya existe un tercero con el legajo '{request.Legajo}'.");

        if (await repo.ExisteNroDocumentoAsync(request.NroDocumento, null, ct))
            return Result.Failure<long>($"Ya existe un tercero con el documento '{request.NroDocumento}'.");

        var tercero = Tercero.Crear(
            request.Legajo,
            request.RazonSocial,
            request.TipoDocumentoId,
            request.NroDocumento,
            request.CondicionIvaId,
            request.EsCliente,
            request.EsProveedor,
            request.SucursalId,
            currentUser.UserId);

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

        if (request.MonedaId.HasValue)
            tercero.SetMoneda(request.MonedaId.Value);

        if (request.CategoriaId.HasValue)
            tercero.SetCategoria(request.CategoriaId);

        await repo.AddAsync(tercero, ct);
        await uow.SaveChangesAsync(ct);

        return Result.Success(tercero.Id);
    }
}