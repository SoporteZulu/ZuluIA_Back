using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Extras.Commands;

public record UpdateMatriculaCommand(long Id, string? Descripcion, DateOnly? FechaVencimiento)
    : IRequest<Result>;
