using MediatR;
using ZuluIA_Back.Application.Features.Colegio.Services;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Colegio.Commands;

public class UpdateLoteColegioCommandHandler(ColegioService service, IUnitOfWork uow)
    : IRequestHandler<UpdateLoteColegioCommand, Result>
{
    public async Task<Result> Handle(UpdateLoteColegioCommand request, CancellationToken ct)
    {
        try
        {
            await service.ActualizarLoteAsync(request, ct);
            await uow.SaveChangesAsync(ct);
            return Result.Success();
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(ex.Message);
        }
    }
}
