using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.OrdenesPreparacion.Commands;

public record AnularOrdenPreparacionCommand(long Id) : IRequest<Result>;
