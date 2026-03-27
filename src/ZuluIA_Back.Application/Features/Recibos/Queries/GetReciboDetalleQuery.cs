using MediatR;
using ZuluIA_Back.Application.Features.Recibos.DTOs;

namespace ZuluIA_Back.Application.Features.Recibos.Queries;

public record GetReciboDetalleQuery(long Id) : IRequest<ReciboDto?>;
