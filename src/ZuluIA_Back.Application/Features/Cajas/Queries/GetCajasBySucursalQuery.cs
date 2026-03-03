using MediatR;
using ZuluIA_Back.Application.Features.Cajas.DTOs;

namespace ZuluIA_Back.Application.Features.Cajas.Queries;

public record GetCajasBySucursalQuery(long SucursalId) : IRequest<IReadOnlyList<CajaListDto>>;