using MediatR;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.BI;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Extras.Commands;

public class AddCuboCampoCommandHandler(
    IRepository<Cubo> cuboRepo,
    IRepository<CuboCampo> campoRepo,
    IUnitOfWork uow)
    : IRequestHandler<AddCuboCampoCommand, Result<long>>
{
    public async Task<Result<long>> Handle(AddCuboCampoCommand request, CancellationToken ct)
    {
        var cuboExists = await cuboRepo.ExistsAsync(c => c.Id == request.CuboId, ct);
        if (!cuboExists)
            return Result.Failure<long>($"Cubo {request.CuboId} no encontrado.");

        CuboCampo campo;
        try
        {
            campo = CuboCampo.Crear(
                request.CuboId,
                request.SourceName,
                request.Descripcion,
                request.Ubicacion,
                request.Posicion,
                request.Visible ?? true,
                request.Calculado ?? false,
                request.Filtro,
                request.CampoPadreId,
                request.Orden ?? 0);
        }
        catch (ArgumentException ex)
        {
            return Result.Failure<long>(ex.Message);
        }

        await campoRepo.AddAsync(campo, ct);
        await uow.SaveChangesAsync(ct);

        return Result.Success(campo.Id);
    }
}
