using MediatR;
using ZuluIA_Back.Application.Features.Colegio.Services;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Colegio.Commands;

public class RegistrarCobinproColegioCommandHandler(ColegioService service, IUnitOfWork uow)
    : IRequestHandler<RegistrarCobinproColegioCommand, Result<long>>
{
    public async Task<Result<long>> Handle(RegistrarCobinproColegioCommand request, CancellationToken ct)
    {
        try
        {
            var operacion = await service.RegistrarCobinproAsync(request, ct);
            await uow.SaveChangesAsync(ct);
            return Result.Success(operacion.Id);
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure<long>(ex.Message);
        }
    }
}
