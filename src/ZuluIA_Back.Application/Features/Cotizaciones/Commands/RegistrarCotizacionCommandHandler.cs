using MediatR;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Finanzas;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Cotizaciones.Commands;

public class RegistrarCotizacionCommandHandler(
    ICotizacionMonedaRepository repo,
    IUnitOfWork uow)
    : IRequestHandler<RegistrarCotizacionCommand, Result<long>>
{
    public async Task<Result<long>> Handle(RegistrarCotizacionCommand request, CancellationToken ct)
    {
        // Si ya existe para esa fecha, actualizamos en lugar de duplicar
        var existente = await repo.FirstOrDefaultAsync(
            x => x.MonedaId == request.MonedaId && x.Fecha == request.Fecha, ct);

        if (existente is not null)
        {
            existente.ActualizarCotizacion(request.Cotizacion);
            repo.Update(existente);
            await uow.SaveChangesAsync(ct);
            return Result.Success(existente.Id);
        }

        var cotizacion = CotizacionMoneda.Crear(
            request.MonedaId,
            request.Fecha,
            request.Cotizacion);

        await repo.AddAsync(cotizacion, ct);
        await uow.SaveChangesAsync(ct);

        return Result.Success(cotizacion.Id);
    }
}