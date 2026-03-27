using FluentValidation;

namespace ZuluIA_Back.Application.Features.Facturacion.Commands;

public class CrearOrdenCargaCommandValidator : AbstractValidator<CrearOrdenCargaCommand>
{
    public CrearOrdenCargaCommandValidator()
    {
        RuleFor(x => x.CartaPorteId).GreaterThan(0);
        RuleFor(x => x.Origen).NotEmpty().MaximumLength(250);
        RuleFor(x => x.Destino).NotEmpty().MaximumLength(250);
    }
}
