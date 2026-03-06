using ZuluIA_Back.Domain.Entities.Contabilidad;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Domain.Services;

/// <summary>
/// Servicio de dominio que centraliza la lógica de generación
/// y validación de asientos contables.
/// </summary>
public class ContabilidadService(
    IAsientoRepository asientoRepo,
    IEjercicioRepository ejercicioRepo)
{
    /// <summary>
    /// Crea y persiste un asiento contable completo validando
    /// que el ejercicio esté abierto y que el asiento cuadre.
    /// </summary>
    public async Task<Asiento> RegistrarAsientoAsync(
        long ejercicioId,
        long sucursalId,
        DateOnly fecha,
        string descripcion,
        string? origenTabla,
        long? origenId,
        IReadOnlyList<(long CuentaId, decimal Debe, decimal Haber,
                       string? Desc, long? CentroCostoId)> lineas,
        long? userId,
        CancellationToken ct = default)
    {
        // 1. Validar ejercicio abierto
        var ejercicio = await ejercicioRepo.GetByIdAsync(ejercicioId, ct)
            ?? throw new InvalidOperationException(
                $"No se encontró el ejercicio ID {ejercicioId}.");

        if (ejercicio.Cerrado)
            throw new InvalidOperationException(
                $"El ejercicio '{ejercicio.Descripcion}' está cerrado.");

        if (!ejercicio.ContienesFecha(fecha))
            throw new InvalidOperationException(
                $"La fecha {fecha} no pertenece al ejercicio '{ejercicio.Descripcion}'.");

        // 2. Obtener próximo número
        var numero = await asientoRepo.GetProximoNumeroAsync(
            ejercicioId, sucursalId, ct);

        // 3. Crear asiento
        var asiento = Asiento.Crear(
            ejercicioId, sucursalId, fecha,
            numero, descripcion,
            origenTabla, origenId, userId);

        // 4. Agregar líneas
        short orden = 0;
        foreach (var (cuentaId, debe, haber, desc, centroCostoId) in lineas)
        {
            var linea = AsientoLinea.Crear(
                0, cuentaId, debe, haber,
                desc, orden++, centroCostoId);

            asiento.AgregarLinea(linea);
        }

        // 5. Confirmar (valida que cuadre)
        asiento.Confirmar();

        await asientoRepo.AddAsync(asiento, ct);
        return asiento;
    }
}