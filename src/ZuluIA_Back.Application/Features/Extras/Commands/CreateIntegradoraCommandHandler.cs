using MediatR;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Extras;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Extras.Commands;

public class CreateIntegradoraCommandHandler(
    IRepository<Integradora> repo,
    IUnitOfWork uow,
    ICurrentUserService currentUser)
    : IRequestHandler<CreateIntegradoraCommand, Result<long>>
{
    public async Task<Result<long>> Handle(CreateIntegradoraCommand request, CancellationToken ct)
    {
        var codigo = request.Codigo.Trim().ToUpperInvariant();
        var exists = await repo.ExistsAsync(x => x.Codigo == codigo, ct);
        if (exists)
            return Result.Failure<long>("Ya existe una integradora con ese codigo.");

        Integradora entity;
        try
        {
            entity = Integradora.Crear(
                codigo,
                request.Nombre,
                request.TipoSistema,
                request.UrlEndpoint,
                request.ApiKey,
                request.Configuracion,
                currentUser.UserId);
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