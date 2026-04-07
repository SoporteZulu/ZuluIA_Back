using MediatR;
using ZuluIA_Back.Application.Features.Colegio.Services;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Colegio.Commands;

public class CreateLoteColegioCommandHandler(ColegioService service, IUnitOfWork uow)
    : IRequestHandler<CreateLoteColegioCommand, Result<long>>
{
    public async Task<Result<long>> Handle(CreateLoteColegioCommand request, CancellationToken ct)
    {
        try
        {
            var lote = await service.CrearLoteAsync(request, ct);
            await uow.SaveChangesAsync(ct);
            return Result.Success(lote.Id);
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure<long>(ex.Message);
        }
    }
}
