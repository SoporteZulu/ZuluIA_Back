namespace ZuluIA_Back.Application.Features.Stock.DTOs;

public sealed record ConteoCiclicoDto(
    long Id,
    string Deposito,
    string Zona,
    string Frecuencia,
    DateOnly ProximoConteo,
    string Estado,
    decimal DivergenciaPct,
    string Responsable,
    string Observacion,
    string NextStep,
    string ExecutionNote);
