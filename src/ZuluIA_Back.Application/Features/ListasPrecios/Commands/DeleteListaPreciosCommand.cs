using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.ListasPrecios.Commands;

public record DeleteListaPreciosCommand(long Id) : IRequest<Result>;