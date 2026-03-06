using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Produccion.Commands;

public record IngredienteInput(
    long ItemId,
    decimal Cantidad,
    long? UnidadMedidaId,
    bool EsOpcional,
    short Orden);

public record CreateFormulaProduccionCommand(
    string Codigo,
    string Descripcion,
    long ItemResultadoId,
    decimal CantidadResultado,
    long? UnidadMedidaId,
    string? Observacion,
    IReadOnlyList<IngredienteInput> Ingredientes
) : IRequest<Result<long>>;