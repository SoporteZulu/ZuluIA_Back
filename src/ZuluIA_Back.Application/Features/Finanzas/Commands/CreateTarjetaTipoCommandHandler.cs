using MediatR;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Finanzas;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Finanzas.Commands;

public class CreateTarjetaTipoCommandHandler(
    IRepository<TarjetaTipo> repo,
    IUnitOfWork uow)
    : IRequestHandler<CreateTarjetaTipoCommand, Result<long>>
{
    public async Task<Result<long>> Handle(CreateTarjetaTipoCommand request, CancellationToken ct)
    {
        var codigo = request.Codigo.Trim().ToUpperInvariant();
        var exists = await repo.ExistsAsync(x => x.Codigo == codigo, ct);
        if (exists)
            return Result.Failure<long>("Ya existe una tarjeta con ese codigo.");

        TarjetaTipo entity;
        try
        {
            entity = TarjetaTipo.Crear(codigo, request.Descripcion, request.EsDebito, userId: null);
        }
        catch (ArgumentException ex)
        {
            return Result.Failure<long>(ex.Message);
        }

        await repo.AddAsync(entity, ct);
        await uow.SaveChangesAsync(ct);

        return Result.Success(entity.Id);
    }
}
