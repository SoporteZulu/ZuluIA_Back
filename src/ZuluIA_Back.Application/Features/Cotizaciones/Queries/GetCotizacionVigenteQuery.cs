using MediatR;
using ZuluIA_Back.Application.Features.Cotizaciones.DTOs;

namespace ZuluIA_Back.Application.Features.Cotizaciones.Queries;

public record GetCotizacionVigenteQuery(long MonedaId, DateOnly Fecha) : IRequest<CotizacionMonedaDto?>;