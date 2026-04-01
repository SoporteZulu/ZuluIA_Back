using FluentValidation;
using ZuluIA_Back.Application.Features.Ventas.Services;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Application.Features.Ventas.Commands;

public class RegistrarDevolucionVentaCommandValidator : AbstractValidator<RegistrarDevolucionVentaCommand>
{
    private readonly NotaCreditoValidationService _ncValidationService;

    public RegistrarDevolucionVentaCommandValidator(NotaCreditoValidationService ncValidationService)
    {
        _ncValidationService = ncValidationService;

        RuleFor(x => x.SucursalId).GreaterThan(0);
        RuleFor(x => x.TipoComprobanteId).GreaterThan(0);
        RuleFor(x => x.TerceroId).GreaterThan(0);
        RuleFor(x => x.MonedaId).GreaterThan(0);
        RuleFor(x => x.Cotizacion).GreaterThan(0);
        RuleFor(x => x.Items).NotEmpty();
        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.ItemId).GreaterThan(0);
            item.RuleFor(i => i.Cantidad).GreaterThan(0);
            item.RuleFor(i => i.AlicuotaIvaId).GreaterThan(0);
        });

        // Validación de motivo de devolución
        RuleFor(x => x.MotivoDevolucion)
            .IsInEnum()
            .WithMessage("El motivo de devolución debe ser válido.");

        RuleFor(x => x.ObservacionDevolucion)
            .MaximumLength(1000)
            .WithMessage("La observación de devolución no puede exceder 1000 caracteres.");

        // Si requiere autorización, debe tener autorizador
        When(x => x.MotivoDevolucion == MotivoDevolucion.Garantia || 
                  x.MotivoDevolucion == MotivoDevolucion.DiferenciaPrecio ||
                  x.MotivoDevolucion == MotivoDevolucion.AjusteInventario, () =>
        {
            RuleFor(x => x.AutorizadorDevolucionId)
                .NotNull()
                .GreaterThan(0)
                .WithMessage("Este tipo de devolución requiere autorización de un supervisor.");
        });

        // Validación asíncrona contra factura origen
        When(x => x.ComprobanteOrigenId.HasValue, () =>
        {
            RuleFor(x => x)
                .MustAsync(async (command, ct) =>
                {
                    var total = command.Items.Sum(i => i.Cantidad * i.PrecioUnitario);
                    var itemsValidacion = command.Items.Select(i => new ValidacionItemNC(i.ItemId, i.Cantidad)).ToList();
                    
                    var error = await _ncValidationService.ValidateNotaCreditoAgainstFacturaAsync(
                        command.ComprobanteOrigenId,
                        command.TerceroId,
                        command.MonedaId,
                        total,
                        itemsValidacion,
                        ct);

                    return error is null;
                })
                .WithMessage((command, _) => 
                {
                    var total = command.Items.Sum(i => i.Cantidad * i.PrecioUnitario);
                    var itemsValidacion = command.Items.Select(i => new ValidacionItemNC(i.ItemId, i.Cantidad)).ToList();
                    
                    // Obtener mensaje de error sincrónicamente (no ideal pero necesario para FluentValidation)
                    var error = _ncValidationService.ValidateNotaCreditoAgainstFacturaAsync(
                        command.ComprobanteOrigenId,
                        command.TerceroId,
                        command.MonedaId,
                        total,
                        itemsValidacion,
                        CancellationToken.None).GetAwaiter().GetResult();
                    
                    return error ?? "La nota de crédito no es válida contra la factura origen.";
                });
        });
    }
}
