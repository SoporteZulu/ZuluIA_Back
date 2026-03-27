using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Colegio.Commands;

public record ConciliarCobinproColegioCommand(long Id, bool Confirmar, string? Observacion = null) : IRequest<Result>;
