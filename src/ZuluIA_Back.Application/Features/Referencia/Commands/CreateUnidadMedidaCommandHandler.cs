using MediatR;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Referencia;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Referencia.Commands;

public class CreateUnidadMedidaCommandHandler(
    IRepository<UnidadMedida> repo,
    IUnitOfWork uow)
    : IRequestHandler<CreateUnidadMedidaCommand, Result<long>>
{
    public async Task<Result<long>> Handle(CreateUnidadMedidaCommand request, CancellationToken ct)
    {
        var codigo = request.Codigo.Trim().ToUpperInvariant();
        var existe = await repo.ExistsAsync(x => x.Codigo == codigo, ct);
        if (existe)
            return Result.Failure<long>($"Ya existe una unidad de medida con codigo '{request.Codigo}'.");

        UnidadMedida entity;
        try
        {
            entity = UnidadMedida.Crear(
                request.Codigo,
                request.Descripcion,
                request.Disminutivo,
                request.Multiplicador,
                request.EsUnidadBase,
                request.UnidadBaseId);
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