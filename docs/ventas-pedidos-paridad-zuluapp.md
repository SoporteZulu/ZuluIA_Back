# Paridad Funcional: Ventas > Pedidos vs zuluApp

## Objetivo
Lograr **paridad funcional 100%** entre la vista de pedidos del legado `C:\Zulu\zuluApp` (formulario `frmNotaPedido` y vistas relacionadas) y el nuevo frontend `C:\Zulu\ZuluIA_Front\app\ventas\pedidos`, cerrando primero todos los gaps del backend `C:\Zulu\ZuluIA_Back`.

## Regla de oro
**`zuluApp` es la ley**: todos los campos visibles, acciones, estados, filtros y trazabilidad del legado deben estar 100% cubiertos en el backend antes de considerar cerrado el circuito de pedidos.

---

## 1. Análisis del legado `zuluApp`

### 1.1 Formularios identificados
- **`frmNotaPedido`**: formulario principal de alta/edición/consulta de pedidos
- **`VTAESTADONOTASPEDIDO_Listado.asp`**: grilla de consulta de pedidos y su estado
- **`VTACERRARNOTASPEDIDO_*.asp`**: flujos de cierre de pedidos (masivo, individual, todas)
- **`VTAConsultaNotaPedidoVinculadas_Listado.asp`**: consulta de pedidos vinculados
- **`VTAESTADONOTASPEDIDOEXPEDICION_Listado.asp`**: consulta de pedidos desde perspectiva de expedición

### 1.2 Campos identificados del legado

#### Campos de encabezado del pedido (comprobante principal)
```sql
-- De VTAESTADONOTAPEDIDO y COMPROBANTES
ID_NP                           -- Id del pedido
NroNP                           -- Número de pedido (prefijo + número)
prefijo                         -- Sucursal/punto de emisión
fechacomprobante                -- Fecha de emisión del pedido
FechaEntrega                    -- Fecha de entrega comprometida
Cliente                         -- Razón social del cliente
legajo                          -- Legajo del cliente
cuit_cuil                       -- CUIT/CUIL del cliente
condiva                         -- Condición IVA del cliente
moneda                          -- Moneda del pedido
cotizacion                      -- Cotización de la moneda
totalcondescuentogeneral        -- Total del pedido con descuento
estadonp                        -- Estado del pedido: 0=Pendiente, 1=Cerrada, 2=Completado
ANULADA                         -- Si el comprobante está anulado (0=no, 1=sí)
```

#### Campos de detalle del pedido (items/renglones)
```sql
ID_ITEM_NP                      -- Id del item del pedido
Id_Item                         -- Id del producto/item
codigo                          -- Código del producto
Item                            -- Descripción del producto
concepto                        -- Concepto/observación del renglón
Cant                            -- Cantidad pedida
CantSec                         -- Cantidad en unidad secundaria (si aplica)
estado_item                     -- Estado del renglón: 0=No Entregado, 1=Entrega Completa, 2=Entrega Parcial, -1=Sobrepasada
atraso                          -- Si hay atraso en la entrega (0=No, 1=Sí)
Retrasado                       -- Descripción: 'Si' o 'No'
EstadoEntregaItem               -- Descripción legible del estado del item
diferencia                      -- Diferencia entre cantidad pedida y entregada
```

### 1.3 Estados operativos identificados

#### Estado del pedido (encabezado)
- **0 - Pendiente**: pedido creado, esperando cumplimiento
- **1 - Cerrada**: pedido cerrado manualmente o por proceso masivo
- **2 - Completado**: pedido cumplido totalmente

#### Estado del item (renglón)
- **0 - No Entregado**: el renglón no tiene entregas asociadas
- **1 - Entrega Completa**: se entregó la cantidad exacta pedida
- **2 - Entrega Parcial**: se entregó menos de lo pedido
- **-1 - Entrega Sobrepasada**: se entregó más de lo pedido

#### Indicador de atraso
- **0 - No**: el pedido/renglón no está atrasado respecto a fecha de entrega
- **1 - Sí**: el pedido/renglón está atrasado

### 1.4 Operaciones identificadas
- **Crear pedido**: alta de nota de pedido con encabezado y detalle
- **Modificar pedido pendiente**: edición si aún no se empezó a cumplir
- **Anular pedido**: invalidar el pedido
- **Cerrar pedido individual**: marcar como cerrado sin cumplir totalmente
- **Cerrar pedidos masivo**: cerrar varios pedidos filtrados
- **Cerrar todas las notas de pedido**: cierre masivo total
- **Consultar estado**: listar pedidos con filtros por cliente, fecha, estado, item, atraso, sucursal
- **Consultar vinculadas**: ver documentos generados a partir del pedido (remitos, facturas)
- **Registrar entrega**: al emitir remitos vinculados, actualiza `estado_item` y `diferencia`
- **Reimpresión**: regenerar el pedido impreso

### 1.5 Filtros operativos relevados
- **Razón social** (cliente)
- **Sucursal** (prefijo)
- **Fecha de entrega** (desde - hasta)
- **Número de pedido**
- **Estado del item** (0,1,2,-1)
- **Código o concepto de producto**
- **Atraso** (Sí/No)
- **Fecha comprobante** (desde - hasta)
- **Estado del pedido** (0,1,2)

---

## 2. Análisis del backend actual `ZuluIA_Back`

### 2.1 Entidades de dominio relevantes

#### `Comprobante` (src/ZuluIA_Back.Domain/Entities/Comprobantes/Comprobante.cs)
- ✅ Contiene: `Id`, `Numero`, `Fecha`, `TerceroId`, `Estado`, `Total`, `Saldo`, `ComprobanteOrigenId`
- ✅ Contiene datos comerciales: `VendedorId`, `CobradorId`, `ZonaComercialId`, `ListaPreciosId`, `CondicionPagoId`, `PlazoDias`, `CanalVentaId`
- ✅ Contiene datos logísticos: `TransporteId`, `FechaEstimadaEntrega`, `FechaRealEntrega`, `DomicilioEntrega`, `ObservacionesLogisticas`
- ❌ **NO tiene**: campo específico de `EstadoPedido` (pendiente/cerrado/completado)
- ❌ **NO tiene**: campo `FechaEntregaCompromiso` separado de `FechaEstimadaEntrega`
- ❌ **NO tiene**: campo de `Atraso` calculado o persistido

#### `ComprobanteItem` (src/ZuluIA_Back.Domain/Entities/Comprobantes/ComprobanteItem.cs)
- ✅ Contiene: `Id`, `ItemId`, `Cantidad`, `PrecioUnitario`, `Subtotal`
- ❌ **NO tiene**: `EstadoEntrega` (no entregado/parcial/completo/sobrepasado)
- ❌ **NO tiene**: `CantidadEntregada` o `CantidadPendiente`
- ❌ **NO tiene**: `Diferencia` calculada
- ❌ **NO tiene**: `Atraso` por renglón

### 2.2 Comandos/Queries existentes

#### Comandos disponibles
- ✅ **`CrearBorradorVentaCommand`**: crea comprobante pero sin estado específico de pedido
- ✅ **`EmitirDocumentoVentaCommand`**: emite comprobante formal
- ✅ **`ConvertirDocumentoVentaCommand`**: convierte entre tipos documentales (presupuesto → pedido → remito → factura)
- ✅ **`VincularComprobanteVentaCommand`**: vincula documentos
- ❌ **NO existe**: `CerrarPedidoCommand`
- ❌ **NO existe**: `CerrarPedidosMasivoCommand`
- ❌ **NO existe**: `ActualizarEstadoEntregaPedidoCommand` (para actualizar al emitir remitos)

#### Queries disponibles
- ✅ **`GetComprobantesPagedQuery`**: listado con filtros básicos
- ✅ **`GetComprobanteDetalleQuery`**: detalle completo del comprobante
- ❌ **NO existe**: query específica de pedidos con `EstadoEntrega`, `Atraso`, `Diferencia`
- ❌ **NO existe**: query de pedidos vinculados/cumplimiento

### 2.3 DTOs existentes

#### `ComprobanteDetalleDto`
- ✅ Contiene datos comerciales y logísticos completos
- ❌ **NO expone**: `EstadoPedido` específico
- ❌ **NO expone**: `Atraso` booleano o calculado
- ❌ **NO expone**: métricas de cumplimiento

#### `ComprobanteItemDto`
- ✅ Contiene datos del item básico
- ❌ **NO expone**: `EstadoEntrega`, `CantidadEntregada`, `CantidadPendiente`, `Diferencia`, `Atraso`

### 2.4 Enums existentes

#### `EstadoComprobante`
```csharp
public enum EstadoComprobante
{
    Borrador = 0,
    Emitido = 1,
    Anulado = 2,
    Pendiente = 3
}
```
- ✅ Tiene `Borrador`, `Emitido`, `Anulado`, `Pendiente`
- ❌ **NO tiene**: estados específicos de pedido como `Cerrado`, `Completado`, `EnProceso`

---

## 3. Gaps identificados (backend-first)

### 3.1 Gaps de entidades de dominio

#### Falta agregar a `Comprobante`:
1. **`EstadoPedido`** (enum):
   - Valores: `Pendiente`, `EnProceso`, `Completado`, `Cerrado`, `Anulado`
2. **`FechaEntregaCompromiso`** (`DateOnly?`):
   - Fecha comprometida con el cliente (distinta de `FechaEstimadaEntrega` logística)
3. **Método de dominio**: `CerrarPedido()` - cierra el pedido sin cumplir totalmente
4. **Método de dominio**: `ActualizarCumplimientoPedido()` - recalcula estado según entregas

#### Falta agregar a `ComprobanteItem`:
1. **`CantidadEntregada`** (`decimal`): cantidad ya cumplida de este renglón
2. **`CantidadPendiente`** (`decimal`): cantidad que falta entregar
3. **`EstadoEntregaItem`** (enum):
   - Valores: `NoEntregado`, `EntregaParcial`, `EntregaCompleta`, `EntregaSobrepasada`
4. **`EsAtrasado`** (`bool`): indica si está atrasado respecto a fecha de entrega
5. **Método de dominio**: `RegistrarEntrega(decimal cantidadEntregada)` - actualiza cantidades y estado

### 3.2 Gaps de enums

#### Crear `EstadoPedido`:
```csharp
public enum EstadoPedido
{
    Pendiente = 0,      // Pedido creado, sin entregas
    EnProceso = 1,      // Tiene entregas parciales
    Completado = 2,     // Todas las entregas cumplidas
    Cerrado = 3,        // Cerrado manualmente sin cumplir
    Anulado = 4         // Anulado
}
```

#### Crear `EstadoEntregaItem`:
```csharp
public enum EstadoEntregaItem
{
    NoEntregado = 0,              // Sin entregas
    EntregaParcial = 1,           // Entregado < Pedido
    EntregaCompleta = 2,          // Entregado == Pedido
    EntregaSobrepasada = 3        // Entregado > Pedido
}
```

### 3.3 Gaps de Commands

#### Crear `CerrarPedidoCommand`:
```csharp
public record CerrarPedidoCommand(
    long PedidoId,
    string? MotivoCierre
) : IRequest<Result>;
```

#### Crear `CerrarPedidosMasivoCommand`:
```csharp
public record CerrarPedidosMasivoCommand(
    long? SucursalId,
    long? ClienteId,
    DateOnly? FechaDesde,
    DateOnly? FechaHasta,
    bool SoloPendientes,
    string? MotivoCierre
) : IRequest<Result<int>>; // devuelve cantidad cerrada
```

#### Modificar `EmitirRemito` (proceso existente):
- Debe invocar automáticamente `ActualizarCumplimientoPedidoCommand` si el remito proviene de un pedido

#### Crear `ActualizarCumplimientoPedidoCommand`:
```csharp
public record ActualizarCumplimientoPedidoCommand(
    long PedidoId
) : IRequest<Result>;
```
- Recalcula `CantidadEntregada`, `CantidadPendiente`, `EstadoEntregaItem` y `EstadoPedido` revisando los remitos vinculados

### 3.4 Gaps de Queries

#### Crear `GetPedidosConEstadoQuery`:
```csharp
public record GetPedidosConEstadoQuery(
    long? SucursalId,
    long? ClienteId,
    DateOnly? FechaDesde,
    DateOnly? FechaHasta,
    DateOnly? FechaEntregaDesde,
    DateOnly? FechaEntregaHasta,
    EstadoPedido? EstadoPedido,
    EstadoEntregaItem? EstadoEntregaItem,
    bool? SoloAtrasados,
    long? ItemId,
    string? CodigoOConcepto,
    int PageNumber = 1,
    int PageSize = 20
) : IRequest<Result<PagedResult<PedidoConEstadoDto>>>;
```

#### Crear `GetPedidoVinculacionesQuery`:
```csharp
public record GetPedidoVinculacionesQuery(
    long PedidoId
) : IRequest<Result<PedidoVinculacionesDto>>;
```
- Devuelve: remitos generados, facturas asociadas, cumplimiento por renglón

### 3.5 Gaps de DTOs

#### Crear `PedidoConEstadoDto`:
```csharp
public class PedidoConEstadoDto
{
    public long Id { get; set; }
    public string NumeroFormateado { get; set; } = string.Empty;
    public DateOnly Fecha { get; set; }
    public DateOnly? FechaEntregaCompromiso { get; set; }
    public string ClienteRazonSocial { get; set; } = string.Empty;
    public string ClienteLegajo { get; set; } = string.Empty;
    public string ClienteCuit { get; set; } = string.Empty;
    public string MonedaSimbolo { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public EstadoPedido EstadoPedido { get; set; }
    public string EstadoPedidoDescripcion { get; set; } = string.Empty;
    public bool EsAtrasado { get; set; }
    public int CantidadRenglones { get; set; }
    public int RenglonesCompletados { get; set; }
    public int RenglonesPendientes { get; set; }
    public decimal PorcentajeCumplimiento { get; set; }
    public IReadOnlyList<PedidoItemConEstadoDto> Items { get; set; } = [];
}
```

#### Crear `PedidoItemConEstadoDto`:
```csharp
public class PedidoItemConEstadoDto
{
    public long Id { get; set; }
    public long ItemId { get; set; }
    public string ItemCodigo { get; set; } = string.Empty;
    public string ItemDescripcion { get; set; } = string.Empty;
    public string? Concepto { get; set; }
    public decimal CantidadPedida { get; set; }
    public decimal CantidadEntregada { get; set; }
    public decimal CantidadPendiente { get; set; }
    public decimal Diferencia { get; set; }
    public EstadoEntregaItem EstadoEntrega { get; set; }
    public string EstadoEntregaDescripcion { get; set; } = string.Empty;
    public bool EsAtrasado { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Subtotal { get; set; }
}
```

#### Crear `PedidoVinculacionesDto`:
```csharp
public class PedidoVinculacionesDto
{
    public long PedidoId { get; set; }
    public string PedidoNumero { get; set; } = string.Empty;
    public EstadoPedido EstadoPedido { get; set; }
    public IReadOnlyList<ComprobanteVinculadoDto> RemitosGenerados { get; set; } = [];
    public IReadOnlyList<ComprobanteVinculadoDto> FacturasAsociadas { get; set; } = [];
    public IReadOnlyList<CumplimientoRenglonDto> CumplimientoPorRenglon { get; set; } = [];
}

public class ComprobanteVinculadoDto
{
    public long Id { get; set; }
    public string Numero { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public DateOnly Fecha { get; set; }
    public string Estado { get; set; } = string.Empty;
}

public class CumplimientoRenglonDto
{
    public long ItemId { get; set; }
    public string ItemCodigo { get; set; } = string.Empty;
    public string ItemDescripcion { get; set; } = string.Empty;
    public decimal CantidadPedida { get; set; }
    public decimal CantidadEntregada { get; set; }
    public decimal Diferencia { get; set; }
    public IReadOnlyList<EntregaDetalleDto> Entregas { get; set; } = [];
}

public class EntregaDetalleDto
{
    public long RemitoId { get; set; }
    public string RemitoNumero { get; set; } = string.Empty;
    public DateOnly FechaRemito { get; set; }
    public decimal CantidadEntregada { get; set; }
}
```

### 3.6 Gaps de configuración/persistencia

#### Actualizar `ComprobanteConfiguration`:
- Agregar mapping para `EstadoPedido`, `FechaEntregaCompromiso`
- Crear índices sobre `EstadoPedido` y `FechaEntregaCompromiso`

#### Actualizar `ComprobanteItemConfiguration`:
- Agregar mapping para `CantidadEntregada`, `CantidadPendiente`, `EstadoEntregaItem`, `EsAtrasado`
- Crear índices sobre `EstadoEntregaItem`

### 3.7 Gaps de validaciones

#### Crear `CerrarPedidoCommandValidator`:
- Validar que el pedido exista y no esté anulado
- Validar que no esté ya cerrado o completado
- Validar permisos del usuario

#### Crear `CerrarPedidosMasivoCommandValidator`:
- Validar que al menos un filtro esté presente
- Validar fechas si se especifican

---

## 4. Plan de implementación (backend-first)

### Fase 1: Extender entidades de dominio y enums
1. Crear enum `EstadoPedido` en `src/ZuluIA_Back.Domain/Enums/`
2. Crear enum `EstadoEntregaItem` en `src/ZuluIA_Back.Domain/Enums/`
3. Extender `Comprobante` con:
   - Propiedad `EstadoPedido?`
   - Propiedad `FechaEntregaCompromiso`
   - Método `CerrarPedido(string? motivo)`
   - Método `ActualizarCumplimientoPedido()`
4. Extender `ComprobanteItem` con:
   - Propiedad `CantidadEntregada`
   - Propiedad `CantidadPendiente`
   - Propiedad `EstadoEntregaItem?`
   - Propiedad `EsAtrasado`
   - Método `RegistrarEntrega(decimal cantidad)`

### Fase 2: Actualizar configuración de persistencia
1. Migración de base de datos:
   - Agregar columnas a `comprobantes`: `estado_pedido`, `fecha_entrega_compromiso`
   - Agregar columnas a `comprobantes_items`: `cantidad_entregada`, `cantidad_pendiente`, `estado_entrega_item`, `es_atrasado`
   - Crear índices sobre nuevas columnas
2. Actualizar `ComprobanteConfiguration.cs`
3. Actualizar `ComprobanteItemConfiguration.cs`

### Fase 3: Implementar Commands
1. `CerrarPedidoCommand` + Handler + Validator
2. `CerrarPedidosMasivoCommand` + Handler + Validator
3. `ActualizarCumplimientoPedidoCommand` + Handler
4. Modificar `EmitirRemitoCommandHandler` para que invoque `ActualizarCumplimientoPedidoCommand` si hay pedido origen

### Fase 4: Implementar Queries y DTOs
1. Crear DTOs: `PedidoConEstadoDto`, `PedidoItemConEstadoDto`, `PedidoVinculacionesDto` y auxiliares
2. `GetPedidosConEstadoQuery` + Handler
3. `GetPedidoVinculacionesQuery` + Handler
4. Extender `ComprobanteDetalleDto` con campos de pedido

## Estado de implementación actual

### Backend fase 3-4 completada
- `CrearBorradorVentaCommand` ahora acepta `FechaEntregaCompromiso` y metadatos comerciales del pedido
- `ConvertirDocumentoVentaCommand` ahora acepta `FechaEntregaCompromiso` y preserva `ComprobanteItemOrigenId` por renglón
- `EmitirDocumentoVentaCommandHandler` recalcula automáticamente el cumplimiento cuando se emite un remito vinculado a pedido
- `ConvertirDocumentoVentaCommandHandler` recalcula automáticamente el cumplimiento cuando la conversión genera un remito desde pedido
- nuevos commands implementados:
  - `CerrarPedidoCommand`
  - `CerrarPedidosMasivoCommand`
  - `ActualizarCumplimientoPedidoCommand`
- nuevas queries implementadas:
  - `GetPedidosConEstadoQuery`
  - `GetPedidoVinculacionesQuery`
- nuevos DTOs implementados:
  - `PedidoConEstadoDto`
  - `PedidoItemConEstadoDto`
  - `PedidoVinculacionesDto`
  - `ComprobanteVinculadoDto`
  - `CumplimientoRenglonDto`
  - `EntregaDetalleDto`
- nuevos endpoints API expuestos en `VentasController`:
  - `GET /api/ventas/pedidos`
  - `GET /api/ventas/pedidos/{id}/vinculaciones`
  - `POST /api/ventas/pedidos/{id}/cerrar`
  - `POST /api/ventas/pedidos/cerrar-masivo`
  - `POST /api/ventas/pedidos/{id}/actualizar-cumplimiento`

### Base de datos diferida
- por decisión del usuario, el ajuste físico de BD queda diferido para una etapa posterior
- la capa de aplicación y el contrato API ya quedaron preparados para continuar la paridad funcional

### Fase 5: Exponer endpoints en API
1. Crear o extender `PedidosController`:
   - `GET /api/pedidos` → `GetPedidosConEstadoQuery`
   - `GET /api/pedidos/{id}/vinculaciones` → `GetPedidoVinculacionesQuery`
   - `POST /api/pedidos/{id}/cerrar` → `CerrarPedidoCommand`
   - `POST /api/pedidos/cerrar-masivo` → `CerrarPedidosMasivoCommand`
   - `POST /api/pedidos/{id}/actualizar-cumplimiento` → `ActualizarCumplimientoPedidoCommand`

### Fase 6: Tests
1. Tests de dominio: `EstadoPedido`, `EstadoEntregaItem`, métodos de `Comprobante` y `ComprobanteItem`
2. Tests de Commands: todos los handlers y validators
3. Tests de Queries: handlers y mapeo de DTOs
4. Tests de integración API

### Fase 7: Documentación y verificación
1. Actualizar matriz funcional de ventas con estado `REAL` para pedidos
2. Crear smoke tests de pedidos contra base local
3. Documentar flujo completo de pedido → remito → factura con trazabilidad
4. Marcar como cerrado cuando frontend nuevo pueda reemplazar totalmente al legado

---

## 5. Criterios de paridad 100% lograda

- ✅ El backend expone todos los campos visibles en `frmNotaPedido` y `VTAESTADONOTASPEDIDO_Listado.asp`
- ✅ Se pueden crear pedidos con `FechaEntregaCompromiso`
- ✅ Se pueden cerrar pedidos individual y masivamente
- ✅ Se puede consultar el estado de cumplimiento de cada pedido y renglón
- ✅ Al emitir remitos vinculados, se actualizan automáticamente `CantidadEntregada`, `CantidadPendiente`, `EstadoEntregaItem` y `EstadoPedido`
- ✅ Se detecta automáticamente el atraso comparando `FechaEntregaCompromiso` con `DateTime.Now`
- ✅ Se pueden filtrar pedidos por: sucursal, cliente, fecha pedido, fecha entrega, estado pedido, estado entrega item, atraso, item/código
- ✅ Se puede consultar la vinculación completa: pedido → remitos → facturas
- ✅ Los DTOs devuelven: `Diferencia`, `PorcentajeCumplimiento`, `RenglonesCompletados`, `RenglonesPendientes`
- ✅ El frontend NO depende de colecciones locales ni overlay de datos
- ✅ La UX nueva puede reemplazar 1:1 al legado sin pérdida funcional

---

## 6. Campos del legado vs backend (tabla de mapeo)

| Campo legacy | Tabla | Campo backend | Ubicación | Estado |
|---|---|---|---|---|
| `ID_NP` | `COMPROBANTES` | `Id` | `Comprobante.Id` | ✅ |
| `NroNP` | | `Numero` | `Comprobante.Numero` | ✅ |
| `prefijo` | | `Numero.Prefijo` | `NroComprobante.Prefijo` | ✅ |
| `fechacomprobante` | | `Fecha` | `Comprobante.Fecha` | ✅ |
| `FechaEntrega` | | `FechaEntregaCompromiso` | **🚧 FALTA** | ❌ |
| `Cliente` | `TERCEROS` | `TerceroRazonSocial` | DTO | ✅ |
| `legajo` | | `TerceroLegajo` | DTO | ✅ |
| `cuit_cuil` | | `TerceroCuit` | DTO | ✅ |
| `condiva` | | `TerceroCondicionIva` | DTO | ✅ |
| `moneda` | `MONEDAS` | `MonedaSimbolo` | DTO | ✅ |
| `cotizacion` | | `Cotizacion` | `Comprobante.Cotizacion` | ✅ |
| `totalcondescuentogeneral` | | `Total` | `Comprobante.Total` | ✅ |
| `estadonp` | | `EstadoPedido` | **🚧 FALTA** | ❌ |
| `ANULADA` | | `Estado` | `Comprobante.Estado` | ✅ |
| `ID_ITEM_NP` | `COMPROBANTES_ITEMS` | `Id` | `ComprobanteItem.Id` | ✅ |
| `Id_Item` | | `ItemId` | `ComprobanteItem.ItemId` | ✅ |
| `codigo` | `ITEMS` | `ItemCodigo` | DTO | ✅ |
| `Item` | | `ItemDescripcion` | DTO | ✅ |
| `concepto` | | `Concepto` | `ComprobanteItem.Concepto` | ✅ |
| `Cant` | | `Cantidad` | `ComprobanteItem.Cantidad` | ✅ |
| `CantSec` | | (unidad secundaria) | **🚧 REVISAR** | ⚠️ |
| `estado_item` | | `EstadoEntregaItem` | **🚧 FALTA** | ❌ |
| `atraso` | | `EsAtrasado` | **🚧 FALTA** | ❌ |
| `Retrasado` | | (descripción) | DTO | ❌ |
| `EstadoEntregaItem` | | (descripción) | DTO | ❌ |
| `diferencia` | | `Diferencia` = `Cantidad - CantidadEntregada` | **🚧 FALTA** | ❌ |
| | | `CantidadEntregada` | **🚧 FALTA** | ❌ |
| | | `CantidadPendiente` | **🚧 FALTA** | ❌ |

**Leyenda**:
- ✅ Existe y está completo
- ❌ No existe, debe implementarse
- ⚠️ Existe pero debe revisarse/extenderse
- 🚧 Work in progress

---

## 7. Resumen ejecutivo

### Estado actual
- El backend tiene la base documental general (`Comprobante` y `ComprobanteItem`)
- Faltan campos, estados y operaciones específicas de **pedido** como entidad operativa distinta
- No hay forma de cerrar pedidos, consultar cumplimiento por renglón, ni actualizar estado de entrega automáticamente

### Ruta crítica
1. **Primero**: extender el dominio con `EstadoPedido`, `EstadoEntregaItem` y campos de cumplimiento
2. **Segundo**: crear Commands de cierre y actualización de cumplimiento
3. **Tercero**: crear Queries específicas de pedidos con estado
4. **Cuarto**: exponer endpoints en API
5. **Quinto**: validar con smoke tests contra zuluApp

### Impacto
- **Alto**: pedidos son la puerta de entrada del circuito de ventas
- Sin pedidos operativos completos, el frontend no puede mostrar pipeline comercial ni gestionar cumplimiento
- Afecta: remitos, facturas, reportes comerciales, alertas operativas

### Prioridad
**🔥 CRÍTICA**: los pedidos son core del módulo de ventas y su estado de cumplimiento es el indicador operativo principal para el negocio.

---

**Próximos pasos**: implementar Fase 1 (dominio y enums) y Fase 2 (persistencia) antes de continuar.
