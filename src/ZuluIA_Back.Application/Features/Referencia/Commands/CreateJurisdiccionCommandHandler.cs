using MediatR;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Referencia;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Referencia.Commands;

public class CreateJurisdiccionCommandHandler(
    IRepository<Jurisdiccion> repo,
    IUnitOfWork uow)
    : IRequestHandler<CreateJurisdiccionCommand, Result<long>>
{
    public async Task<Result<long>> Handle(CreateJurisdiccionCommand request, CancellationToken ct)
    {
        Jurisdiccion entity;
        try
        {
            entity = Jurisdiccion.Crear(request.Descripcion);
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