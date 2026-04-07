using MediatR;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Referencia;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Referencia.Commands;

public class CreateMarcaCommandHandler(
    IRepository<Marca> repo,
    IUnitOfWork uow)
    : IRequestHandler<CreateMarcaCommand, Result<long>>
{
    public async Task<Result<long>> Handle(CreateMarcaCommand request, CancellationToken ct)
    {
        Marca entity;
        try
        {
            entity = Marca.Crear(request.Descripcion);
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