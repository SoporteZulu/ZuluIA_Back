using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Extras.Commands;

public record CreateMatriculaCommand(
    long TerceroId,
    long SucursalId,
    string NroMatricula,
    string? Descripcion,
    DateOnly FechaAlta,
    DateOnly? FechaVencimiento) : IRequest<Result<long>>;
