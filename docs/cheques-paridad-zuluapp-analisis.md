# Análisis de Paridad Funcional: Cheques ZuluApp → ZuluIA_Back

## Fecha de Análisis
2024-03-20

## Objetivo
Lograr la paridad funcional total entre la vista de ventas/cheques de `C:\Zulu\zuluApp` y el backend moderno `ZuluIA_Back`, asegurando que todos los campos, operaciones y funcionalidades estén cubiertas.

---

## 1. Estado Actual del Backend (ZuluIA_Back)

### 1.1 Entidad Cheque
**Ubicación:** `src\ZuluIA_Back.Domain\Entities\Finanzas\Cheque.cs`

**Campos Actuales:**
- `Id` (long)
- `CajaId` (long)
- `TerceroId` (long?)
- `NroCheque` (string)
- `Banco` (string)
- `FechaEmision` (DateOnly)
- `FechaVencimiento` (DateOnly)
- `FechaAcreditacion` (DateOnly?)
- `FechaDeposito` (DateOnly?)
- `Importe` (decimal)
- `MonedaId` (long)
- `Estado` (EstadoCheque enum)
- `Observacion` (string?)
- Campos de auditoría: `CreatedAt`, `UpdatedAt`, `CreatedBy`, `UpdatedBy`

**Métodos Actuales:**
- `Crear()` - Crea un cheque en estado Cartera
- `Depositar()` - Cambia estado a Depositado
- `Acreditar()` - Cambia estado a Acreditado
- `Rechazar()` - Cambia estado a Rechazado
- `Entregar()` - Cambia estado a Entregado
- `SetObservacion()` - Actualiza observación

**Estados Actuales (EstadoCheque):**
- Cartera
- Depositado
- Acreditado
- Rechazado
- Entregado

### 1.2 Operaciones Disponibles
**Comandos:**
- `CreateChequeCommand` - Crear cheque
- `CambiarEstadoChequeCommand` - Cambiar estado (depositar, acreditar, rechazar)

**Queries:**
- `GetChequesPagedQuery` - Listar cheques con filtros

**Endpoints API:**
- `GET /api/cheques` - Listado paginado
- `GET /api/cheques/cartera/{cajaId}` - Cheques en cartera
- `POST /api/cheques` - Crear cheque
- `POST /api/cheques/{id}/depositar` - Depositar
- `POST /api/cheques/{id}/acreditar` - Acreditar
- `POST /api/cheques/{id}/rechazar` - Rechazar

---

## 2. Funcionalidad en ZuluApp (Proyecto Viejo)

### 2.1 Vistas Identificadas
Basado en el análisis de archivos ASP en `C:\Zulu\zuluApp\ASP\`:

1. **FROCHEQUESCARTERA_Listado.asp** - Cheques en cartera
2. **FROCHEQUESDEPOSITADOS_Listado.asp** - Cheques depositados
3. **FROCHEQUESEMITIDOS_Listado.asp** - Cheques emitidos (propios)
4. **FROCHEQUESPENDIENTES_Listado.asp** - Cheques pendientes
5. **FROCHEQUESPENDIENTESDETALLE_Listado.asp** - Detalle de pendientes
6. **FROCHEQUESPROPIOSAnulados_Listado.asp** - Cheques propios anulados
7. **FROCHEQUESPROPIOS_Listado.asp** - Cheques propios
8. **FROCHEQUESTERCEROS_Listado.asp** - Cheques de terceros
9. **FRORUTACHEQUECARTERA_LISTADO.asp** - Ruta/historial de un cheque
10. **FROEDITARCHEQUESPROPIOS_EditarForm.asp** - Editar cheques propios
11. **FROEDITARCHEQUESTERCEROS_EditarForm.asp** - Editar cheques terceros
12. **CHEQUERAS_Listado.asp** - Gestión de chequeras
13. **VTACONCEPTOSCHEQUESRECHAZADOS_Listado.asp** - Conceptos de rechazo

### 2.2 Campos Identificados en ZuluApp

**FROCHEQUESTERCEROS (Cheques de Terceros):**
- `id` (PK)
- `id_banco` - ID del banco
- `Codigosucursal` - Código de sucursal bancaria
- `Codigopostal` - Código postal del cheque
- `NroDocumento` - Número del documento/cheque
- `FECHA_ALTA` - Fecha de alta/registro
- `Fechaemision` - Fecha de emisión
- `Fechavencimiento` - Fecha de vencimiento
- `Total` - Importe del cheque
- `Titular` - Nombre del titular del cheque
- `RazonSocial` - Razón social asociada
- `ALaOrden` - Indicador si es "A la orden" (SI/NO)
- `Cruzado` - Indicador si está cruzado (SI/NO)
- `Estado` - Estado del cheque (texto)
- `id_estado` - ID del estado (FK)

**FRORUTACHEQUECARTERA (Ruta/Historial):**
- `Caja` - Descripción de la caja
- `FechaComprobante` - Fecha del comprobante que lo generó
- `Comprobante` - Tipo de comprobante
- `NroComprobante` - Número del comprobante
- `Prefijo` - Prefijo del comprobante
- `Concepto` - Concepto/descripción de la operación
- `Estado` - Estado en ese momento
- `FormaPago` - Forma de pago asociada
- `id_comprobante` - ID del comprobante relacionado

**Campos Adicionales Observados:**
- Relación con comprobantes (origen)
- Historial de movimientos del cheque
- Concepto de rechazo (en caso de rechazados)
- Estado detallado con descripción
- Clasificación: Propio vs. Tercero
- Chequera de origen (para cheques propios)

### 2.3 Operaciones en ZuluApp

**Cheques de Terceros (Recibidos):**
1. Registrar ingreso
2. Ver en cartera
3. Depositar
4. Acreditar
5. Rechazar (con concepto)
6. Endosar/Entregar a tercero
7. Ver ruta/historial completo
8. Editar datos del cheque
9. Filtrar por múltiples criterios

**Cheques Propios (Emitidos):**
1. Emitir desde chequera
2. Anular
3. Ver emitidos
4. Ver anulados
5. Editar cheque propio
6. Gestión de chequeras

**Consultas Avanzadas:**
1. Cheques pendientes de depósito
2. Cheques depositados no acreditados
3. Cheques rechazados con conceptos
4. Filtros por:
   - Caja
   - Banco
   - Estado
   - Rango de fechas (emisión/vencimiento)
   - Titular
   - Número de documento
   - Razón social

---

## 3. Gaps Identificados

### 3.1 Campos Faltantes en Entidad

| Campo ZuluApp | Equivalente Backend | Status | Prioridad |
|---------------|---------------------|---------|-----------|
| `Codigosucursal` | ❌ No existe | **FALTA** | Alta |
| `Codigopostal` | ❌ No existe | **FALTA** | Alta |
| `Titular` | ❌ No existe | **FALTA** | **CRÍTICA** |
| `ALaOrden` | ❌ No existe | **FALTA** | Alta |
| `Cruzado` | ❌ No existe | **FALTA** | Alta |
| `RazonSocial` | Derivable de TerceroId | OK (puede proyectarse) | - |
| `id_banco` | Podría ser parte de Banco (normalizar) | **EVALUAR** | Media |
| `id_estado` | `Estado` (enum) | OK | - |
| `FECHA_ALTA` | `CreatedAt` | OK | - |

**Campos Adicionales Necesarios:**
- `TipoCheque` (enum: Propio, Tercero)
- `ChequeraId` (long?, para cheques propios)
- `NumeroCheque` (puede ser diferente de NroCheque en propios)
- `CodigoSucursalBancaria` (string?)
- `CodigoPostal` (string?)
- `Titular` (string) - **CRÍTICO**
- `EsALaOrden` (bool)
- `EsCruzado` (bool)
- `ConceptoRechazo` (string?)
- `ComprobanteOrigenId` (long?) - Comprobante que generó el cheque

### 3.2 Estados Faltantes

**ZuluApp tiene estados más detallados:**
- Pendiente de depósito
- En tránsito
- Anulado (para propios)
- Endosado
- En cartera (múltiples terceros)

**Propuesta de Extensión:**
```csharp
public enum EstadoCheque
{
    Cartera,         // Existente
    Depositado,      // Existente
    Acreditado,      // Existente
    Rechazado,       // Existente
    Entregado,       // Existente
    Anulado,         // NUEVO - para cheques propios
    Endosado,        // NUEVO - específico para endoso
    EnTransito       // NUEVO - entre depósito y acreditación
}
```

### 3.3 Operaciones Faltantes

| Operación ZuluApp | Backend Actual | Status |
|-------------------|----------------|---------|
| Ver ruta/historial completo | ❌ | **FALTA** |
| Endosar cheque | ❌ | **FALTA** |
| Anular cheque propio | ❌ | **FALTA** |
| Emitir desde chequera | ❌ | **FALTA** |
| Registrar concepto de rechazo | Observación genérica | **MEJORAR** |
| Filtro por "A la orden" | ❌ | **FALTA** |
| Filtro por "Cruzado" | ❌ | **FALTA** |
| Filtro por titular | ❌ | **FALTA** |
| Query de cheques propios | ❌ | **FALTA** |
| Query de cheques terceros | Existe genérico | **OK** |
| Query de pendientes detalle | ❌ | **FALTA** |

### 3.4 DTOs y Queries Faltantes

**DTOs a crear/extender:**
1. `ChequeDetalleDto` - Con toda la información incluyendo historial
2. `ChequeRutaDto` - Para el historial de movimientos
3. `ChequePendienteDto` - Para vista de pendientes
4. `ChequePropio específico` - Información de chequera
5. Extender `ChequeDto` con campos nuevos

**Queries a implementar:**
1. `GetChequeDetalleQuery` - Detalle completo con historial
2. `GetChequesPropiosPagedQuery` - Cheques propios específicamente
3. `GetChequesPendientesQuery` - Pendientes de depósito
4. `GetChequesDepositadosQuery` - Depositados no acreditados
5. `GetChequeHistorialQuery` - Historial completo de movimientos

**Comandos a implementar:**
1. `EndosarChequeCommand` - Endosar a tercero
2. `AnularChequePropioCommand` - Anular cheque emitido
3. `EmitirChequePropioCommand` - Emitir desde chequera
4. `RechazarChequeConConceptoCommand` - Rechazar con concepto específico

### 3.5 Relaciones Faltantes

**Entidades relacionadas necesarias:**
1. `ChequeHistorial` - ✅ Ya existe (revisar si es completo)
2. `ConceptoRechazo` - ❌ Crear catálogo
3. `Chequera` - ✅ Ya existe
4. Relación con `Comprobante` - ❌ Agregar FK

---

## 4. Plan de Implementación Propuesto

### Fase 1: Extender Entidad y DTOs (Prioridad Alta) ✅ ACTUAL
1. Agregar campos faltantes a `Cheque`
2. Extender `EstadoCheque` enum
3. Crear/actualizar DTOs completos
4. Crear `ConceptoRechazo` como catálogo

### Fase 2: Queries y Filtros Avanzados
5. Implementar `GetChequeDetalleQuery`
6. Crear queries específicas (propios, pendientes, etc.)
7. Extender filtros en `GetChequesPagedQuery`

### Fase 3: Operaciones Adicionales
8. Implementar `EndosarChequeCommand`
9. Implementar `AnularChequePropioCommand`
10. Implementar `EmitirChequePropioCommand`
11. Mejorar `RechazarChequeCommand` con concepto

### Fase 4: Historial y Auditoría
12. Extender `ChequeHistorial` si necesario
13. Implementar `GetChequeHistorialQuery`
14. Asegurar trazabilidad completa

### Fase 5: Testing y Documentación
15. Tests unitarios completos
16. Tests de integración
17. Documentación API
18. Validación con zuluApp

---

## 5. Decisiones Técnicas

### 5.1 Normalización de Banco
**Opción A:** Mantener `Banco` como string libre
**Opción B:** Crear tabla `Bancos` normalizada

**Decisión:** Mantener string libre por ahora, pero agregar campos estructurados opcionales (CodigoSucursal, CodigoPostal)

### 5.2 Tipo de Cheque
Agregar enum `TipoCheque`:
```csharp
public enum TipoCheque
{
    Tercero,  // Recibido
    Propio    // Emitido
}
```

### 5.3 Historial
El `ChequeHistorial` actual cubre las operaciones básicas. Necesita:
- Registrar comprobante de origen
- Capturar endosos
- Registrar concepto de rechazo detallado

---

## 6. Criterios de Aceptación

La paridad funcional se considerará completa cuando:

✅ **Campos:**
- [ ] Todos los campos visibles en zuluApp están en el backend
- [ ] Titular, A la orden, Cruzado están implementados
- [ ] Códigos bancarios (sucursal, postal) disponibles

✅ **Operaciones:**
- [ ] Endosar cheque funciona correctamente
- [ ] Anular cheque propio implementado
- [ ] Emitir desde chequera funciona
- [ ] Concepto de rechazo registrado

✅ **Consultas:**
- [ ] Filtros avanzados (titular, a la orden, cruzado)
- [ ] Vista de cheques propios separada
- [ ] Vista de cheques pendientes con detalle
- [ ] Historial/ruta completo visible

✅ **Testing:**
- [ ] Tests unitarios cubren nuevas operaciones
- [ ] Tests de integración verifican flujos completos
- [ ] Validación contra datos de zuluApp

---

## 7. Riesgos y Consideraciones

### Riesgos:
1. **Migración de datos existentes** - Cheques sin campos nuevos
   - Solución: Campos opcionales (nullable) con valores por defecto

2. **Compatibilidad con frontend actual** - DTOs extendidos
   - Solución: Versionado de API o campos opcionales

3. **Complejidad del historial** - Muchos eventos por cheque
   - Solución: Queries optimizadas y proyecciones selectivas

### Consideraciones:
- Mantener simplicidad en la API
- No sobre-normalizar (balance con zuluApp)
- Priorizar funcionalidad visible al usuario
- Documentar diferencias con zuluApp cuando existan

---

## 8. Referencias

**Archivos ZuluApp Analizados:**
- `/ASP/FROCHEQUESTERCEROS_Listado.asp`
- `/ASP/FRORUTACHEQUECARTERA_LISTADO.asp`
- `/ASP/FROEDITARCHEQUESTERCEROS_EditarForm.asp`
- `/ASP/FROCHEQUESPROPIOS_Listado.asp`
- `/ASP/FROCHEQUESPROPIOSAnular_*.asp`

**Archivos Backend Revisados:**
- `src\ZuluIA_Back.Domain\Entities\Finanzas\Cheque.cs`
- `src\ZuluIA_Back.Application\Features\Cheques\Commands\*.cs`
- `src\ZuluIA_Back.Application\Features\Cheques\Queries\*.cs`
- `src\ZuluIA_Back.Api\Controllers\ChequesController.cs`

---

**Siguiente Acción:** Implementar Fase 1 - Extender entidad `Cheque` y DTOs
