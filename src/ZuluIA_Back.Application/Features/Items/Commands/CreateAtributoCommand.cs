using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Items.Commands;

public record CreateAtributoCommand(string Descripcion, string Tipo = "texto", bool Requerido = false)
    : IRequest<Result<long>>;