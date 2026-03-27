using ZuluIA_Back.Domain.Entities.Finanzas;
using ZuluIA_Back.Domain.Enums;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Cheques.Services;

public class ChequeAuditoriaService(IRepository<ChequeHistorial> historialRepo)
{
    public async Task RegistrarAsync(
        Cheque cheque,
        TipoOperacionCheque operacion,
        EstadoCheque? estadoAnterior,
        DateOnly fechaOperacion,
        DateOnly? fechaAcreditacion,
        long? terceroId,
        string? observacion,
        long? userId,
        CancellationToken ct = default)
    {
        var historial = ChequeHistorial.Registrar(
            cheque.Id,
            cheque.CajaId,
            terceroId ?? cheque.TerceroId,
            operacion,
            estadoAnterior,
            cheque.Estado,
            fechaOperacion,
            fechaAcreditacion,
            observacion ?? cheque.Observacion,
            userId);

        await historialRepo.AddAsync(historial, ct);
    }
}
