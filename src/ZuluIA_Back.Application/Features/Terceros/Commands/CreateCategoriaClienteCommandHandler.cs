using MediatR;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Terceros;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Terceros.Commands;

public class CreateCategoriaClienteCommandHandler(
    IRepository<CategoriaCliente> repo,
    IUnitOfWork uow)
    : IRequestHandler<CreateCategoriaClienteCommand, Result<long>>
{
    public async Task<Result<long>> Handle(CreateCategoriaClienteCommand request, CancellationToken ct)
    {
        var codigo = request.Codigo.Trim().ToUpperInvariant();
        var exists = await repo.ExistsAsync(x => x.Codigo == codigo, ct);
        if (exists)
            return Result.Failure<long>("Ya existe una categoria de cliente con ese codigo.");

        CategoriaCliente entity;
        try
        {
            entity = CategoriaCliente.Crear(codigo, request.Descripcion, userId: null);
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
