using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Colegio.Commands;

public record CreateLoteColegioCommand(long PlanGeneralColegioId, string Codigo, DateOnly FechaEmision, DateOnly FechaVencimiento, string? Observacion) : IRequest<Result<long>>;
