using MediatR;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Impuestos;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Impuestos.Commands;

public class AssignImpuestoTipoComprobanteCommandHandler(
    IRepository<Impuesto> impuestoRepo,
    IRepository<ImpuestoPorTipoComprobante> asignacionRepo,
    IUnitOfWork uow)
    : IRequestHandler<AssignImpuestoTipoComprobanteCommand, Result<long>>
{
    public async Task<Result<long>> Handle(AssignImpuestoTipoComprobanteCommand request, CancellationToken ct)
    {
        var impuestoExists = await impuestoRepo.ExistsAsync(x => x.Id == request.ImpuestoId, ct);
        if (!impuestoExists)
            return Result.Failure<long>($"Impuesto {request.ImpuestoId} no encontrado.");

        var exists = await asignacionRepo.ExistsAsync(
            x => x.ImpuestoId == request.ImpuestoId && x.TipoComprobanteId == request.TipoComprobanteId,
            ct);
        if (exists)
            return Result.Failure<long>("El tipo de comprobante ya esta asignado a este impuesto.");

        ImpuestoPorTipoComprobante entity;
        try
        {
            entity = ImpuestoPorTipoComprobante.Crear(request.ImpuestoId, request.TipoComprobanteId, request.Orden);
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
