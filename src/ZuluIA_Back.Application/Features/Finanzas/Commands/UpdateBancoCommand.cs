using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Finanzas.Commands;

public record UpdateBancoCommand(long Id, string Descripcion) : IRequest<Result<UpdateBancoResult>>;

public record UpdateBancoResult(long Id, string Descripcion);
