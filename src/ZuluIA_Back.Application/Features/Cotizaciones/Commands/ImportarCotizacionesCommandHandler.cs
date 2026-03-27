using MediatR;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Finanzas;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Cotizaciones.Commands;

public class ImportarCotizacionesCommandHandler(
    ICotizacionMonedaRepository repo,
    IUnitOfWork uow)
    : IRequestHandler<ImportarCotizacionesCommand, Result<ImportarCotizacionesResultDto>>
{
    public async Task<Result<ImportarCotizacionesResultDto>> Handle(ImportarCotizacionesCommand request, CancellationToken ct)
    {
        try
        {
            var creadas = 0;
            var actualizadas = 0;

            foreach (var item in request.Items.OrderBy(x => x.Fecha))
            {
                var existente = await repo.FirstOrDefaultAsync(
                    x => x.MonedaId == request.MonedaId && x.Fecha == item.Fecha,
                    ct);

                if (existente is not null)
                {
                    existente.ActualizarCotizacion(item.Cotizacion);
                    repo.Update(existente);
                    actualizadas++;
                    continue;
                }

                var cotizacion = CotizacionMoneda.Crear(request.MonedaId, item.Fecha, item.Cotizacion);
                await repo.AddAsync(cotizacion, ct);
                creadas++;
            }

            await uow.SaveChangesAsync(ct);

            return Result.Success(new ImportarCotizacionesResultDto(
                request.MonedaId,
                request.Items.Count,
                creadas,
                actualizadas));
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure<ImportarCotizacionesResultDto>(ex.Message);
        }
    }
}