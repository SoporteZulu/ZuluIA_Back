using MediatR;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Contabilidad;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Contabilidad.Commands;

public class CreateCentroCostoCommandHandler(
    IRepository<CentroCosto> repo,
    IUnitOfWork uow)
    : IRequestHandler<CreateCentroCostoCommand, Result<long>>
{
    public async Task<Result<long>> Handle(CreateCentroCostoCommand request, CancellationToken ct)
    {
        var codigo = request.Codigo.Trim().ToUpperInvariant();

        var existe = await repo.ExistsAsync(x => x.Codigo == codigo, ct);
        if (existe)
            return Result.Failure<long>($"Ya existe un centro de costo con codigo '{request.Codigo}'.");

        CentroCosto cc;
        try
        {
            cc = CentroCosto.Crear(request.Codigo, request.Descripcion);
        }
        catch (ArgumentException ex)
        {
            return Result.Failure<long>(ex.Message);
        }

        await repo.AddAsync(cc, ct);
        await uow.SaveChangesAsync(ct);

        return Result.Success(cc.Id);
    }
}