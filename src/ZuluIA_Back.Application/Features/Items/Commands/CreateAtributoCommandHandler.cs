using MediatR;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Items;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Items.Commands;

public class CreateAtributoCommandHandler(
    IRepository<Atributo> repo,
    IUnitOfWork uow)
    : IRequestHandler<CreateAtributoCommand, Result<long>>
{
    public async Task<Result<long>> Handle(CreateAtributoCommand request, CancellationToken ct)
    {
        Atributo entity;
        try
        {
            entity = Atributo.Crear(request.Descripcion, request.Tipo, request.Requerido);
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