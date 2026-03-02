using FluentValidation;

namespace ZuluIA_Back.Application.Features.Terceros.Queries;

public class GetTercerosPagedQueryValidator
    : AbstractValidator<GetTercerosPagedQuery>
{
    public GetTercerosPagedQueryValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1)
            .WithMessage("La página debe ser mayor o igual a 1.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage("El tamaño de página debe estar entre 1 y 100.");

        RuleFor(x => x.Search)
            .MaximumLength(100)
            .When(x => x.Search is not null)
            .WithMessage("El término de búsqueda no puede superar los 100 caracteres.");
    }
}