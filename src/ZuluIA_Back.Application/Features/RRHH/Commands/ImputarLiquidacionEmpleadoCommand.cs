using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.RRHH.Commands;

public record ImputarLiquidacionEmpleadoCommand(long LiquidacionSueldoId, long CajaId, DateOnly Fecha, decimal Importe, string? Observacion, bool GenerarComprobanteSiNoExiste = true) : IRequest<Result<long>>;
