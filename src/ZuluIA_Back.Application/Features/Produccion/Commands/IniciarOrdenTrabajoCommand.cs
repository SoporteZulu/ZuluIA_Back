using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Produccion.Commands;

public record IniciarOrdenTrabajoCommand(long Id) : IRequest<Result>;
