using MediatR;
using ZuluIA_Back.Application.Features.Tesoreria.Services;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Tesoreria.Commands;

public class CerrarCajaTesoreriaCommandHandler(
    TesoreriaService tesoreriaService,
    IUnitOfWork uow)
    : IRequestHandler<CerrarCajaTesoreriaCommand, Result<long>>
{
    public async Task<Result<long>> Handle(CerrarCajaTesoreriaCommand request, CancellationToken ct)
    {
        try
        {
            var cierre = await tesoreriaService.CerrarCajaAsync(
                request.CajaId,
                request.FechaCierre,
                request.SaldoInformado,
                request.Observacion,
                ct);

            await uow.SaveChangesAsync(ct);
            return Result.Success(cierre.Id);
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure<long>(ex.Message);
        }
    }
}
