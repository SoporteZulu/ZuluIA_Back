using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Produccion.Commands;

public record CancelarOrdenTrabajoCommand(long Id) : IRequest<Result>;
