using MediatR;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Facturacion;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Facturacion.Commands;

public class CreateTimbradoCommandHandler(
    IRepository<Timbrado> repo,
    IUnitOfWork uow)
    : IRequestHandler<CreateTimbradoCommand, Result<long>>
{
    public async Task<Result<long>> Handle(CreateTimbradoCommand request, CancellationToken ct)
    {
        Timbrado entity;
        try
        {
            entity = Timbrado.Crear(
                request.SucursalId,
                request.PuntoFacturacionId,
                request.TipoComprobanteId,
                request.NroTimbrado,
                request.FechaInicio,
                request.FechaFin,
                request.NroComprobanteDesde,
                request.NroComprobanteHasta);
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