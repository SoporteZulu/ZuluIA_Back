using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Finanzas.Commands;

public record UpdateTarjetaTipoCommand(long Id, string Codigo, string Descripcion, bool EsDebito)
    : IRequest<Result>;
