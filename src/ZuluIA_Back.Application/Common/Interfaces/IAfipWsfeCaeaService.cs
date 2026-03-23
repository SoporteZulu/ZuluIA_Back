namespace ZuluIA_Back.Application.Common.Interfaces;

public sealed record SolicitarCaeaAfipRequest(int Periodo, short Orden);

public sealed record SolicitarCaeaAfipResponse(
    string NroCaea,
    DateOnly? FechaProceso = null,
    DateOnly? FechaTopeInformar = null);

public interface IAfipWsfeCaeaService
{
    Task<SolicitarCaeaAfipResponse> SolicitarCaeaAsync(
        SolicitarCaeaAfipRequest request,
        CancellationToken cancellationToken = default);
}