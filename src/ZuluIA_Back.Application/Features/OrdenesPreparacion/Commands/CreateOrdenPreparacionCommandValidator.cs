using FluentValidation;
using ZuluIA_Back.Application.Features.OrdenesPreparacion.DTOs;

namespace ZuluIA_Back.Application.Features.OrdenesPreparacion.Commands;

public class CreateOrdenPreparacionCommandValidator : AbstractValidator<CreateOrdenPreparacionCommand>
{
    public CreateOrdenPreparacionCommandValidator()
    {
        RuleFor(x => x.SucursalId).GreaterThan(0).WithMessage("La sucursal es obligatoria.");
        RuleFor(x => x.Fecha).NotEmpty().WithMessage("La fecha es obligatoria.");
        RuleFor(x => x.Detalles).NotEmpty().WithMessage("La orden debe tener al menos un detalle.");
        RuleForEach(x => x.Detalles).ChildRules(d =>
        {
            d.RuleFor(x => x.ItemId).GreaterThan(0).WithMessage("El ítem es obligatorio.");
            d.RuleFor(x => x.DepositoId).GreaterThan(0).WithMessage("El depósito es obligatorio.");
            d.RuleFor(x => x.Cantidad).GreaterThan(0).WithMessage("La cantidad debe ser mayor a 0.");
        });
    }
}
