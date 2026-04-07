using MediatR;
using ZuluIA_Back.Application.Features.Comprobantes.DTOs;

namespace ZuluIA_Back.Application.Features.Ventas.Queries;

public record GetMotivosDebitoQuery(bool SoloActivos = true) : IRequest<IReadOnlyList<MotivoDebitoDto>>;
