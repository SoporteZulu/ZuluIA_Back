using FluentValidation;
using ZuluIA_Back.Application.Features.Caea.Commands;

namespace ZuluIA_Back.Application.Features.Caea.Commands;

public class SolicitarCaeaCommandValidator : AbstractValidator<SolicitarCaeaCommand>
{
    public SolicitarCaeaCommandValidator()
    {
        RuleFor(x => x.PuntoFacturacionId).GreaterThan(0);
        RuleFor(x => x.NroCaea).NotEmpty().MaximumLength(15);
        RuleFor(x => x.FechaDesde).NotEmpty();
        RuleFor(x => x.FechaHasta).NotEmpty().GreaterThanOrEqualTo(x => x.FechaDesde);
        RuleFor(x => x.TipoComprobante).NotEmpty().MaximumLength(5);
        RuleFor(x => x.CantidadAsignada).GreaterThan(0);
    }
}

public class SolicitarCaeaAfipCommandValidator : AbstractValidator<SolicitarCaeaAfipCommand>
{
    public SolicitarCaeaAfipCommandValidator()
    {
        RuleFor(x => x.PuntoFacturacionId).GreaterThan(0);
        RuleFor(x => x.Periodo)
            .InclusiveBetween(200001, 299912)
            .Must(PeriodoTieneMesValido)
            .WithMessage("El periodo debe tener formato AAAAMM válido.");
        RuleFor(x => x.Orden).InclusiveBetween((short)1, (short)2);
        RuleFor(x => x.TipoComprobante).NotEmpty().MaximumLength(5);
        RuleFor(x => x.CantidadAsignada).GreaterThan(0);
    }

    private static bool PeriodoTieneMesValido(int periodo)
    {
        var mes = periodo % 100;
        return mes >= 1 && mes <= 12;
    }
}
