using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Finanzas.Commands;

public record PagarCedulonCommand(long Id, decimal Importe) : IRequest<Result<PagarCedulonResult>>;

public record PagarCedulonResult(decimal ImportePagado, decimal SaldoPendiente, string Estado);