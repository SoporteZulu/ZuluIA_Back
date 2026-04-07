using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Integraciones.Commands;

public record OperacionImportacionInput(string Referencia, string Tipo, string? Payload, bool Exitoso);

public record ImportarOperacionesCommand(
    string Nombre,
    IReadOnlyList<OperacionImportacionInput> Operaciones,
    string? Observacion = null) : IRequest<Result<long>>;
