using MediatR;
using ZuluIA_Back.Application.Features.Comprobantes.DTOs;

namespace ZuluIA_Back.Application.Features.Comprobantes.Queries;

public record GetComprobanteByIdQuery(long Id) : IRequest<ComprobanteDto?>;