using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Items.Commands;

public record SetAtributoItemCommand(long ItemId, long AtributoId, string Valor)
    : IRequest<Result<SetAtributoItemResult>>;

public record SetAtributoItemResult(long Id, bool Actualizado);