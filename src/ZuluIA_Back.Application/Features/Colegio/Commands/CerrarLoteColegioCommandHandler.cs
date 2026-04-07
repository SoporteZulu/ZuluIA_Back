using MediatR;
using ZuluIA_Back.Application.Features.Colegio.Services;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Colegio.Commands;

public class CerrarLoteColegioCommandHandler(ColegioService service, IUnitOfWork uow) : IRequestHandler<CerrarLoteColegioCommand, Result>
{
    public async Task<Result> Handle(CerrarLoteColegioCommand request, CancellationToken ct)
    {
        try
        {
            await service.CerrarLoteAsync(request.Id, request.Observacion, ct);
            await uow.SaveChangesAsync(ct);
            return Result.Success();
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(ex.Message);
        }
    }
}
