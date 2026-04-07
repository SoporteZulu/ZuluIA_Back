using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Extras.Commands;

public record CreateRegionCommand(
    string Codigo,
    string Descripcion,
    long? RegionIntegradoraId,
    int Orden,
    int Nivel,
    string? CodigoEstructura,
    bool EsRegionIntegradora,
    string? Observacion) : IRequest<Result<long>>;
