using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Terceros.Commands;

public record CreateEstadoClienteCommand(string Codigo, string Descripcion, bool Bloquea)
    : IRequest<Result<long>>;
