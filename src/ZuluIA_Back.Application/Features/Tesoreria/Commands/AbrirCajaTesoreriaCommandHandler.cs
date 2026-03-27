using MediatR;
using ZuluIA_Back.Application.Features.Tesoreria.Services;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Tesoreria.Commands;

public class AbrirCajaTesoreriaCommandHandler(
    TesoreriaService tesoreriaService,
    IUnitOfWork uow)
    : IRequestHandler<AbrirCajaTesoreriaCommand, Result<long>>
{
    public async Task<Result<long>> Handle(AbrirCajaTesoreriaCommand request, CancellationToken ct)
    {
        try
        {
            var apertura = await tesoreriaService.AbrirCajaAsync(
                request.CajaId,
                request.FechaApertura,
                request.SaldoInicial,
                request.Observacion,
                ct);

            await uow.SaveChangesAsync(ct);
            return Result.Success(apertura.Id);
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure<long>(ex.Message);
        }
    }
}
