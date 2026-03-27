using MediatR;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Application.Features.Diagnosticos.Commands;

public record OpcionDiagnosticaInput(string Codigo, string Descripcion, decimal ValorNumerico, short Orden);

public record CreateVariableDiagnosticaCommand(
    long AspectoId,
    string Codigo,
    string Descripcion,
    TipoVariableDiagnostica Tipo,
    bool Requerida,
    decimal Peso,
    IReadOnlyList<OpcionDiagnosticaInput>? Opciones
) : IRequest<Result<long>>;
