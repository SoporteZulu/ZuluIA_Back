using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Colegio.Commands;

public record DesactivarPlanGeneralColegioCommand(long Id) : IRequest<Result>;
