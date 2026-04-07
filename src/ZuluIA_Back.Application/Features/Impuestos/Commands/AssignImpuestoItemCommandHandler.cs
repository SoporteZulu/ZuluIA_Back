using MediatR;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Impuestos;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Impuestos.Commands;

public class AssignImpuestoItemCommandHandler(
    IRepository<Impuesto> impuestoRepo,
    IRepository<ImpuestoPorItem> asignacionRepo,
    IUnitOfWork uow)
    : IRequestHandler<AssignImpuestoItemCommand, Result<long>>
{
    public async Task<Result<long>> Handle(AssignImpuestoItemCommand request, CancellationToken ct)
    {
        var impuestoExists = await impuestoRepo.ExistsAsync(x => x.Id == request.ImpuestoId, ct);
        if (!impuestoExists)
            return Result.Failure<long>($"Impuesto {request.ImpuestoId} no encontrado.");

        var exists = await asignacionRepo.ExistsAsync(
            x => x.ImpuestoId == request.ImpuestoId && x.ItemId == request.ItemId,
            ct);
        if (exists)
            return Result.Failure<long>("El impuesto ya esta asignado a ese item.");

        ImpuestoPorItem entity;
        try
        {
            entity = ImpuestoPorItem.Crear(request.ImpuestoId, request.ItemId, request.Descripcion, request.Observacion);
        }
        catch (ArgumentException ex)
        {
            return Result.Failure<long>(ex.Message);
        }

        await asignacionRepo.AddAsync(entity, ct);
        await uow.SaveChangesAsync(ct);

        return Result.Success(entity.Id);
    }
}
