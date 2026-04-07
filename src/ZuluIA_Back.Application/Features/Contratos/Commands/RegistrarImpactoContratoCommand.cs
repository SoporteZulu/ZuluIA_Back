using MediatR;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Application.Features.Contratos.Commands;

public record RegistrarImpactoContratoCommand(
    long ContratoId,
    TipoImpactoContrato Tipo,
    DateOnly Fecha,
    decimal Importe,
    string Descripcion,
    bool ImpactarCuentaCorriente = false) : IRequest<Result<long>>;
