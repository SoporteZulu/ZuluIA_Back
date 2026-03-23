using MediatR;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Finanzas;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Finanzas.Commands;

public class CreateBancoCommandHandler(
    IRepository<Banco> repo,
    IUnitOfWork uow)
    : IRequestHandler<CreateBancoCommand, Result<long>>
{
    public async Task<Result<long>> Handle(CreateBancoCommand request, CancellationToken ct)
    {
        var normalized = request.Descripcion.Trim();
        var exists = await repo.ExistsAsync(x => x.Descripcion == normalized, ct);
        if (exists)
            return Result.Failure<long>("Ya existe un banco con esa descripcion.");

        Banco banco;
        try
        {
            banco = Banco.Crear(normalized);
        }
        catch (ArgumentException ex)
        {
            return Result.Failure<long>(ex.Message);
        }

        await repo.AddAsync(banco, ct);
        await uow.SaveChangesAsync(ct);

        return Result.Success(banco.Id);
    }
}
