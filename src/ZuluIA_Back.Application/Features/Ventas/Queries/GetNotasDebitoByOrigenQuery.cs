using MediatR;
using ZuluIA_Back.Application.Features.Comprobantes.DTOs;

namespace ZuluIA_Back.Application.Features.Ventas.Queries;

public record GetNotasDebitoByOrigenQuery(long ComprobanteOrigenId) : IRequest<IReadOnlyList<ComprobanteListDto>>;
