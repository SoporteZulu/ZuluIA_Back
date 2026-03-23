using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Contabilidad.Commands;

public record CreateCentroCostoCommand(string Codigo, string Descripcion) : IRequest<Result<long>>;