using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Finanzas.Commands;

public record UsarChequeCommand(long Id, int Numero) : IRequest<Result<UsarChequeResult>>;

public record UsarChequeResult(long Id, int UltimoChequeUsado);