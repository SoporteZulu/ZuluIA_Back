using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.OrdenesPreparacion.Commands;

public record IniciarOrdenPreparacionCommand(long Id) : IRequest<Result>;
