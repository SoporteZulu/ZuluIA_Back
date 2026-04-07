namespace ZuluIA_Back.Application.Features.Produccion.Commands;

public record ConsumoOrdenTrabajoInput(
    long ItemId,
    decimal CantidadConsumida,
    string? Observacion = null);
