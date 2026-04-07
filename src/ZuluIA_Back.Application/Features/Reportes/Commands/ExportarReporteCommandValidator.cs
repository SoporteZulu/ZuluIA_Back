using FluentValidation;
using ZuluIA_Back.Application.Features.Reportes.Enums;

namespace ZuluIA_Back.Application.Features.Reportes.Commands;

public class ExportarReporteCommandValidator : AbstractValidator<ExportarReporteCommand>
{
    public ExportarReporteCommandValidator()
    {
        RuleFor(x => x.Hasta).GreaterThanOrEqualTo(x => x.Desde);

        RuleFor(x => x.SucursalId)
            .Must(x => x.HasValue && x.Value > 0)
            .When(x => x.TipoReporte is TipoReporteParametrizado.Remitos or TipoReporteParametrizado.Operativo)
            .WithMessage("La sucursal es obligatoria para el reporte solicitado.");

        RuleFor(x => x.EjercicioId)
            .Must(x => x.HasValue && x.Value > 0)
            .When(x => x.TipoReporte == TipoReporteParametrizado.Contable)
            .WithMessage("El ejercicio es obligatorio para el reporte contable.");
    }
}
