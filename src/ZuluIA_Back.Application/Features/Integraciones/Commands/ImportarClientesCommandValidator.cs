using FluentValidation;

namespace ZuluIA_Back.Application.Features.Integraciones.Commands;

public class ImportarClientesCommandValidator : AbstractValidator<ImportarClientesCommand>
{
    public ImportarClientesCommandValidator()
    {
        RuleFor(x => x.Clientes).NotEmpty();
        RuleForEach(x => x.Clientes).ChildRules(cliente =>
        {
            cliente.RuleFor(x => x.Legajo).NotEmpty();
            cliente.RuleFor(x => x.RazonSocial).NotEmpty();
            cliente.RuleFor(x => x.TipoDocumentoId).GreaterThan(0);
            cliente.RuleFor(x => x.NroDocumento).NotEmpty();
            cliente.RuleFor(x => x.CondicionIvaId).GreaterThan(0);
        });
    }
}
