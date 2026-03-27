using FluentValidation;

namespace ZuluIA_Back.Application.Features.Terceros.Commands;

public class UpsertTerceroPerfilComercialCommandValidator : AbstractValidator<UpsertTerceroPerfilComercialCommand>
{
    private static readonly string[] RiesgosValidos = ["NORMAL", "ALERTA", "BLOQUEADO"];

    public UpsertTerceroPerfilComercialCommandValidator()
    {
        RuleFor(x => x.TerceroId)
            .GreaterThan(0);

        RuleFor(x => x.Rubro)
            .MaximumLength(100)
            .When(x => !string.IsNullOrWhiteSpace(x.Rubro));

        RuleFor(x => x.Subrubro)
            .MaximumLength(100)
            .When(x => !string.IsNullOrWhiteSpace(x.Subrubro));

        RuleFor(x => x.Sector)
            .MaximumLength(100)
            .When(x => !string.IsNullOrWhiteSpace(x.Sector));

        RuleFor(x => x.CondicionCobranza)
            .MaximumLength(150)
            .When(x => !string.IsNullOrWhiteSpace(x.CondicionCobranza));

        RuleFor(x => x.RiesgoCrediticio)
            .NotEmpty()
            .Must(x => RiesgosValidos.Contains(x.Trim().ToUpperInvariant()))
            .WithMessage("El riesgo crediticio debe ser NORMAL, ALERTA o BLOQUEADO.");

        RuleFor(x => x.SaldoMaximoVigente)
            .GreaterThanOrEqualTo(0)
            .When(x => x.SaldoMaximoVigente.HasValue);

        RuleFor(x => x.VigenciaSaldo)
            .MaximumLength(100)
            .When(x => !string.IsNullOrWhiteSpace(x.VigenciaSaldo));

        RuleFor(x => x.CondicionVenta)
            .MaximumLength(150)
            .When(x => !string.IsNullOrWhiteSpace(x.CondicionVenta));

        RuleFor(x => x.PlazoCobro)
            .MaximumLength(150)
            .When(x => !string.IsNullOrWhiteSpace(x.PlazoCobro));

        RuleFor(x => x.FacturadorPorDefecto)
            .MaximumLength(150)
            .When(x => !string.IsNullOrWhiteSpace(x.FacturadorPorDefecto));

        RuleFor(x => x.MinimoFacturaMipymes)
            .GreaterThanOrEqualTo(0)
            .When(x => x.MinimoFacturaMipymes.HasValue);

        RuleFor(x => x.ObservacionComercial)
            .MaximumLength(1000)
            .When(x => !string.IsNullOrWhiteSpace(x.ObservacionComercial));
    }
}
