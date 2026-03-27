using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Impuestos.Commands;

public record CreateImpuestoCommand(
    string Codigo,
    string Descripcion,
    decimal Alicuota,
    decimal MinimoBaseCalculo,
    string? Tipo,
    string? Observacion) : IRequest<Result<long>>;