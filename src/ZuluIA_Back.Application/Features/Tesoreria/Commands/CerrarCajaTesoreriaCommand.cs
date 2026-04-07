using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Tesoreria.Commands;

public record CerrarCajaTesoreriaCommand(
    long CajaId,
    DateOnly FechaCierre,
    decimal SaldoInformado,
    string? Observacion
) : IRequest<Result<long>>;
