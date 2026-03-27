using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.RRHH.Commands;

public record GenerarComprobanteEmpleadoCommand(long LiquidacionSueldoId, DateOnly Fecha, string Tipo = "RECIBO_SUELDO", string? Observacion = null) : IRequest<Result<long>>;
