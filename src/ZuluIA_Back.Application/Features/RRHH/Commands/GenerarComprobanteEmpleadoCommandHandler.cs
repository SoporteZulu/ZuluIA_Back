using MediatR;
using ZuluIA_Back.Application.Features.RRHH.Services;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.RRHH.Commands;

public class GenerarComprobanteEmpleadoCommandHandler(RrhhService service, IUnitOfWork uow) : IRequestHandler<GenerarComprobanteEmpleadoCommand, Result<long>>
{
    public async Task<Result<long>> Handle(GenerarComprobanteEmpleadoCommand request, CancellationToken ct)
    {
        try
        {
            var comprobante = await service.GenerarComprobanteEmpleadoAsync(request, ct);
            await uow.SaveChangesAsync(ct);
            return Result.Success(comprobante.Id);
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure<long>(ex.Message);
        }
    }
}
