using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Contabilidad.Commands;

public record CreatePlanCuentaCommand(
    long EjercicioId,
    long? IntegradoraId,
    string CodigoCuenta,
    string Denominacion,
    short Nivel,
    string OrdenNivel,
    bool Imputable,
    string? Tipo,
    char? SaldoNormal
) : IRequest<Result<long>>;