using MediatR;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Impuestos;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Impuestos.Commands;

public class AssignImpuestoTerceroCommandHandler(
    IRepository<Impuesto> impuestoRepo,
    IRepository<ImpuestoPorPersona> asignacionRepo,
    IUnitOfWork uow)
    : IRequestHandler<AssignImpuestoTerceroCommand, Result<long>>
{
    public async Task<Result<long>> Handle(AssignImpuestoTerceroCommand request, CancellationToken ct)
    {
        var impuestoExists = await impuestoRepo.ExistsAsync(x => x.Id == request.ImpuestoId, ct);
        if (!impuestoExists)
            return Result.Failure<long>($"Impuesto {request.ImpuestoId} no encontrado.");

        var exists = await asignacionRepo.ExistsAsync(
            x => x.ImpuestoId == request.ImpuestoId && x.TerceroId == request.TerceroId,
            ct);
        if (exists)
            return Result.Failure<long>("El impuesto ya esta asignado a ese tercero.");

        ImpuestoPorPersona entity;
        try
        {
            entity = ImpuestoPorPersona.Crear(request.ImpuestoId, request.TerceroId, request.Descripcion, request.Observacion);
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
