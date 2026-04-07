using MediatR;
using ZuluIA_Back.Application.Features.Tesoreria.Services;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Enums;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Tesoreria.Commands;

public class RegistrarReintegroTesoreriaCommandHandler(
    TesoreriaService tesoreriaService,
    IUnitOfWork uow)
    : IRequestHandler<RegistrarReintegroTesoreriaCommand, Result<long>>
{
    public async Task<Result<long>> Handle(RegistrarReintegroTesoreriaCommand request, CancellationToken ct)
    {
        try
        {
            var mov = await tesoreriaService.RegistrarMovimientoAsync(
                request.SucursalId,
                request.CajaCuentaId,
                request.Fecha,
                TipoOperacionTesoreria.Reintegro,
                SentidoMovimientoTesoreria.Ingreso,
                request.Importe,
                request.MonedaId,
                request.Cotizacion,
                request.TerceroId,
                "REINTEGRO",
                null,
                request.Observacion,
                ct);

            await uow.SaveChangesAsync(ct);
            return Result.Success(mov.Id);
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure<long>(ex.Message);
        }
    }
}
