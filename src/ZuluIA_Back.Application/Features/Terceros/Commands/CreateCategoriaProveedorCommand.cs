using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Terceros.Commands;

public record CreateCategoriaProveedorCommand(string Codigo, string Descripcion)
    : IRequest<Result<long>>;
