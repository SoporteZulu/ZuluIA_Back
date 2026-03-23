using FluentValidation;

namespace ZuluIA_Back.Application.Features.Finanzas.Commands;

public class CreateBancoCommandValidator : AbstractValidator<CreateBancoCommand>
{
    public CreateBancoCommandValidator()
    {
        RuleFor(x => x.Descripcion).NotEmpty();
    }
}

public class UpdateBancoCommandValidator : AbstractValidator<UpdateBancoCommand>
{
    public UpdateBancoCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Descripcion).NotEmpty();
    }
}

public class DeleteBancoCommandValidator : AbstractValidator<DeleteBancoCommand>
{
    public DeleteBancoCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}
