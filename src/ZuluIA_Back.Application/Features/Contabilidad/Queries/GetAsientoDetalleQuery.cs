using MediatR;
using ZuluIA_Back.Application.Features.Contabilidad.DTOs;

namespace ZuluIA_Back.Application.Features.Contabilidad.Queries;

public record GetAsientoDetalleQuery(long Id) : IRequest<AsientoDto?>;