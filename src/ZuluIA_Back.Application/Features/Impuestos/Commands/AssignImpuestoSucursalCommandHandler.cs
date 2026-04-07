using MediatR;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Impuestos;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Impuestos.Commands;

public class AssignImpuestoSucursalCommandHandler(
    IRepository<Impuesto> impuestoRepo,
    IRepository<ImpuestoPorSucursal> asignacionRepo,
    IUnitOfWork uow)
    : IRequestHandler<AssignImpuestoSucursalCommand, Result<long>>
{
    public async Task<Result<long>> Handle(AssignImpuestoSucursalCommand request, CancellationToken ct)
    {
        var impuestoExists = await impuestoRepo.ExistsAsync(x => x.Id == request.ImpuestoId, ct);
        if (!impuestoExists)
            return Result.Failure<long>($"Impuesto {request.ImpuestoId} no encontrado.");

        var exists = await asignacionRepo.ExistsAsync(
            x => x.ImpuestoId == request.ImpuestoId && x.SucursalId == request.SucursalId,
            ct);
        if (exists)
            return Result.Failure<long>("La sucursal ya esta asignada a este impuesto.");

        ImpuestoPorSucursal entity;
        try
        {
            entity = ImpuestoPorSucursal.Crear(request.ImpuestoId, request.SucursalId, request.Descripcion, request.Observacion);
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
