using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Terceros.Commands;

public record UpdateCategoriaClienteCommand(long Id, string Codigo, string Descripcion)
    : IRequest<Result>;
