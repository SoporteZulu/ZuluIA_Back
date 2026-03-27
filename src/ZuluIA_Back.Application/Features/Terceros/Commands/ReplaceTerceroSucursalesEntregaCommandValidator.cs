using FluentValidation;

namespace ZuluIA_Back.Application.Features.Terceros.Commands;

public class ReplaceTerceroSucursalesEntregaCommandValidator : AbstractValidator<ReplaceTerceroSucursalesEntregaCommand>
{
    public ReplaceTerceroSucursalesEntregaCommandValidator()
    {
        RuleFor(x => x.TerceroId)
            .GreaterThan(0);

        RuleForEach(x => x.Sucursales)
            .SetValidator(new ReplaceTerceroSucursalEntregaItemValidator());

        RuleFor(x => x.Sucursales)
            .Must(items => items.Count(x => x.Principal) <= 1)
            .WithMessage("Solo puede marcarse una sucursal/punto de entrega principal.");
    }
}

public class ReplaceTerceroSucursalEntregaItemValidator : AbstractValidator<ReplaceTerceroSucursalEntregaItem>
{
    public ReplaceTerceroSucursalEntregaItemValidator()
    {
        RuleFor(x => x.Descripcion)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Direccion)
            .MaximumLength(300)
            .When(x => !string.IsNullOrWhiteSpace(x.Direccion));

        RuleFor(x => x.Localidad)
            .MaximumLength(150)
            .When(x => !string.IsNullOrWhiteSpace(x.Localidad));

        RuleFor(x => x.Responsable)
            .MaximumLength(150)
            .When(x => !string.IsNullOrWhiteSpace(x.Responsable));

        RuleFor(x => x.Telefono)
            .MaximumLength(50)
            .When(x => !string.IsNullOrWhiteSpace(x.Telefono));

        RuleFor(x => x.Horario)
            .MaximumLength(150)
            .When(x => !string.IsNullOrWhiteSpace(x.Horario));

        RuleFor(x => x.Orden)
            .GreaterThanOrEqualTo(0)
            .When(x => x.Orden.HasValue);
    }
}
