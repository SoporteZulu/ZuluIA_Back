using MediatR;
using ZuluIA_Back.Application.Features.Comprobantes.DTOs;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Ventas.Commands;

public record ReplaceRemitoAtributosCommand(
    long ComprobanteId,
    IReadOnlyList<RemitoAtributoInput> Atributos) : IRequest<Result<IReadOnlyList<ComprobanteAtributoDto>>>;

public record RemitoAtributoInput(
    string Clave,
    string? Valor,
    string? TipoDato);
