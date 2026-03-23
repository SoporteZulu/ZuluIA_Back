using FluentValidation;

namespace ZuluIA_Back.Application.Features.Terceros.Commands;

public class CreateCategoriaClienteCommandValidator : AbstractValidator<CreateCategoriaClienteCommand>
{
    public CreateCategoriaClienteCommandValidator()
    {
        RuleFor(x => x.Codigo).NotEmpty();
        RuleFor(x => x.Descripcion).NotEmpty();
    }
}

public class UpdateCategoriaClienteCommandValidator : AbstractValidator<UpdateCategoriaClienteCommand>
{
    public UpdateCategoriaClienteCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Codigo).NotEmpty();
        RuleFor(x => x.Descripcion).NotEmpty();
    }
}

public class ActivateCategoriaClienteCommandValidator : AbstractValidator<ActivateCategoriaClienteCommand>
{
    public ActivateCategoriaClienteCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}

public class DeactivateCategoriaClienteCommandValidator : AbstractValidator<DeactivateCategoriaClienteCommand>
{
    public DeactivateCategoriaClienteCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}

public class CreateCategoriaProveedorCommandValidator : AbstractValidator<CreateCategoriaProveedorCommand>
{
    public CreateCategoriaProveedorCommandValidator()
    {
        RuleFor(x => x.Codigo).NotEmpty();
        RuleFor(x => x.Descripcion).NotEmpty();
    }
}

public class UpdateCategoriaProveedorCommandValidator : AbstractValidator<UpdateCategoriaProveedorCommand>
{
    public UpdateCategoriaProveedorCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Codigo).NotEmpty();
        RuleFor(x => x.Descripcion).NotEmpty();
    }
}

public class ActivateCategoriaProveedorCommandValidator : AbstractValidator<ActivateCategoriaProveedorCommand>
{
    public ActivateCategoriaProveedorCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}

public class DeactivateCategoriaProveedorCommandValidator : AbstractValidator<DeactivateCategoriaProveedorCommand>
{
    public DeactivateCategoriaProveedorCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}

public class CreateEstadoClienteCommandValidator : AbstractValidator<CreateEstadoClienteCommand>
{
    public CreateEstadoClienteCommandValidator()
    {
        RuleFor(x => x.Codigo).NotEmpty();
        RuleFor(x => x.Descripcion).NotEmpty();
    }
}

public class UpdateEstadoClienteCommandValidator : AbstractValidator<UpdateEstadoClienteCommand>
{
    public UpdateEstadoClienteCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Codigo).NotEmpty();
        RuleFor(x => x.Descripcion).NotEmpty();
    }
}

public class ActivateEstadoClienteCommandValidator : AbstractValidator<ActivateEstadoClienteCommand>
{
    public ActivateEstadoClienteCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}

public class DeactivateEstadoClienteCommandValidator : AbstractValidator<DeactivateEstadoClienteCommand>
{
    public DeactivateEstadoClienteCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}

public class CreateEstadoProveedorCommandValidator : AbstractValidator<CreateEstadoProveedorCommand>
{
    public CreateEstadoProveedorCommandValidator()
    {
        RuleFor(x => x.Codigo).NotEmpty();
        RuleFor(x => x.Descripcion).NotEmpty();
    }
}

public class UpdateEstadoProveedorCommandValidator : AbstractValidator<UpdateEstadoProveedorCommand>
{
    public UpdateEstadoProveedorCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Codigo).NotEmpty();
        RuleFor(x => x.Descripcion).NotEmpty();
    }
}

public class ActivateEstadoProveedorCommandValidator : AbstractValidator<ActivateEstadoProveedorCommand>
{
    public ActivateEstadoProveedorCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}

public class DeactivateEstadoProveedorCommandValidator : AbstractValidator<DeactivateEstadoProveedorCommand>
{
    public DeactivateEstadoProveedorCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}
