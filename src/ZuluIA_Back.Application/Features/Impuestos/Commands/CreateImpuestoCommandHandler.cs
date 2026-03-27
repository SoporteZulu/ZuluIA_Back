using MediatR;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Impuestos;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Impuestos.Commands;

public class CreateImpuestoCommandHandler(
    IRepository<Impuesto> repo,
    IUnitOfWork uow)
    : IRequestHandler<CreateImpuestoCommand, Result<long>>
{
    public async Task<Result<long>> Handle(CreateImpuestoCommand request, CancellationToken ct)
    {
        Impuesto entity;
        try
        {
            entity = Impuesto.Crear(
                request.Codigo,
                request.Descripcion,
                request.Alicuota,
                request.MinimoBaseCalculo,
                request.Tipo ?? "percepcion",
                request.Observacion);
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