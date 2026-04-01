using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Terceros.Commands;

public record CreateEstadoCivilCommand(string Descripcion)
    : IRequest<Result<long>>;
