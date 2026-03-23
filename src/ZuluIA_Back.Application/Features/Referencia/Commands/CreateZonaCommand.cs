using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Referencia.Commands;

public record CreateZonaCommand(string Descripcion) : IRequest<Result<long>>;