using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Comprobantes.Commands;

public record EmitirComprobanteCommand(
    long Id,
    string? Cae,
    DateOnly? FechaVtoCae
) : IRequest<Result>;