using MediatR;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Facturacion;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Facturacion.Commands;

public class AbrirPeriodoIvaCommandHandler(
    IPeriodoIvaRepository repo,
    IUnitOfWork uow)
    : IRequestHandler<AbrirPeriodoIvaCommand, Result<long>>
{
    public async Task<Result<long>> Handle(
        AbrirPeriodoIvaCommand request,
        CancellationToken ct)
    {
        var existente = await repo.GetPeriodoAsync(request.SucursalId, request.Periodo, ct);

        if (existente is not null)
        {
            if (!existente.Cerrado)
                return Result.Failure<long>(
                    $"El período {request.Periodo:yyyy-MM} ya está abierto.");

            existente.Reabrir();
            repo.Update(existente);
            await uow.SaveChangesAsync(ct);
            return Result.Success(existente.Id);
        }

        var periodo = PeriodoIva.Crear(
            request.EjercicioId,
            request.SucursalId,
            request.Periodo);

        await repo.AddAsync(periodo, ct);
        await uow.SaveChangesAsync(ct);

        return Result.Success(periodo.Id);
    }
}