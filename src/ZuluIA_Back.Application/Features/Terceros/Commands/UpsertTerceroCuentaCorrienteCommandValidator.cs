using FluentValidation;

namespace ZuluIA_Back.Application.Features.Terceros.Commands;

public class UpsertTerceroCuentaCorrienteCommandValidator : AbstractValidator<UpsertTerceroCuentaCorrienteCommand>
{
    public UpsertTerceroCuentaCorrienteCommandValidator()
    {
        RuleFor(x => x.TerceroId)
            .GreaterThan(0);

        RuleFor(x => x.LimiteSaldo)
            .GreaterThanOrEqualTo(0)
            .When(x => x.LimiteSaldo.HasValue);

        RuleFor(x => x.LimiteCreditoTotal)
            .GreaterThanOrEqualTo(0)
            .When(x => x.LimiteCreditoTotal.HasValue);

        RuleFor(x => x)
            .Must(x => !x.VigenciaLimiteSaldoDesde.HasValue || !x.VigenciaLimiteSaldoHasta.HasValue || x.VigenciaLimiteSaldoDesde.Value <= x.VigenciaLimiteSaldoHasta.Value)
            .WithMessage("La vigencia desde del límite de saldo no puede ser mayor que la vigencia hasta.");

        RuleFor(x => x)
            .Must(x => !x.VigenciaLimiteCreditoTotalDesde.HasValue || !x.VigenciaLimiteCreditoTotalHasta.HasValue || x.VigenciaLimiteCreditoTotalDesde.Value <= x.VigenciaLimiteCreditoTotalHasta.Value)
            .WithMessage("La vigencia desde del límite de crédito total no puede ser mayor que la vigencia hasta.");
    }
}
