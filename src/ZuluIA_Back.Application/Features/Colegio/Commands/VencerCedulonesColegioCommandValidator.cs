using FluentValidation;

namespace ZuluIA_Back.Application.Features.Colegio.Commands;

public class VencerCedulonesColegioCommandValidator : AbstractValidator<VencerCedulonesColegioCommand>
{
    public VencerCedulonesColegioCommandValidator()
    {
        RuleFor(x => x).Must(x => x.LoteId.HasValue || x.SucursalId.HasValue)
            .WithMessage("Debe indicar lote o sucursal para ejecutar el vencimiento.");
    }
}
