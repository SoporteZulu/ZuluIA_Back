using MediatR;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Application.Features.Fiscal.Commands;

public record GenerarSalidaRegulatoriaCommand(TipoSalidaRegulatoria Tipo, long SucursalId, DateOnly Desde, DateOnly Hasta, string NombreArchivo) : IRequest<Result<long>>;
