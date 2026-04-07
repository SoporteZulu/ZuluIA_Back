using MediatR;
using ZuluIA_Back.Application.Features.Colegio.Services;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Colegio.Commands;

public class ConciliarCobinproColegioCommandHandler(ColegioService service, IUnitOfWork uow) : IRequestHandler<ConciliarCobinproColegioCommand, Result>
{
    public async Task<Result> Handle(ConciliarCobinproColegioCommand request, CancellationToken ct)
    {
        try
        {
            await service.ConciliarCobinproAsync(request.Id, request.Confirmar, request.Observacion, ct);
            await uow.SaveChangesAsync(ct);
            return Result.Success();
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(ex.Message);
        }
    }
}
