using MediatR;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Contabilidad;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Contabilidad.Commands;

public class CreateAsientoCommandHandler(
    IAsientoRepository repo,
    IUnitOfWork uow,
    ICurrentUserService currentUser)
    : IRequestHandler<CreateAsientoCommand, Result<long>>
{
    public async Task<Result<long>> Handle(CreateAsientoCommand request, CancellationToken ct)
    {
        var numero = await repo.GetProximoNumeroAsync(request.EjercicioId, request.SucursalId, ct);

        var asiento = Asiento.Crear(
            request.EjercicioId,
            request.SucursalId,
            request.Fecha,
            numero,
            request.Descripcion,
            request.OrigenTabla,
            request.OrigenId,
            currentUser.UserId);

        foreach (var lineaDto in request.Lineas.OrderBy(l => l.Orden))
        {
            var linea = AsientoLinea.Crear(
                0,
                lineaDto.CuentaId,
                lineaDto.Debe,
                lineaDto.Haber,
                lineaDto.Descripcion,
                lineaDto.Orden,
                lineaDto.CentroCostoId);

            asiento.AgregarLinea(linea);
        }

        try
        {
            asiento.Contabilizar(currentUser.UserId);
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure<long>(ex.Message);
        }

        await repo.AddAsync(asiento, ct);
        await uow.SaveChangesAsync(ct);

        return Result.Success(asiento.Id);
    }
}