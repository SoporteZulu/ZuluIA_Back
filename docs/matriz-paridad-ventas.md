# Matriz de Paridad Funcional - Sección Ventas
## ZuluIA_Back vs C:\Zulu\zuluApp (Legado) y C:\Zulu\ZuluIA_Front (Nuevo Frontend)

**Fecha de análisis:** 2025-01-20  
**Objetivo:** Documentar el estado de paridad funcional completa del backend de ventas para soportar el frontend nuevo.  
**Scope:** Backend-first, prioridad comercial mínima usable.  
**Referencia funcional principal:** `C:\Zulu\zuluApp`

---

## RESUMEN EJECUTIVO

### Estado General
- **Clientes (Terceros):** ~75% de paridad comercial ✅ Base sólida, gaps menores
- **Productos (Items):** ~80% de paridad comercial ✅ Muy buena cobertura
- **Pedidos:** ~70% de paridad ⚠️ Workflow incompleto, faltan validaciones
- **Remitos:** ~65% de paridad ⚠️ Atributos COT incompletos, gaps en validaciones
- **Facturas:** ~85% de paridad ✅ Muy buena cobertura fiscal
- **Notas de Débito/Crédito:** ~80% de paridad ✅ Funcional, gaps menores

### Gaps Críticos Identificados
1. **Pedidos**: Falta workflow completo de cumplimiento parcial
2. **Remitos**: Atributos COT no completamente implementados
3. **Clientes**: Validaciones de crédito y bloqueos incompletas
4. **Productos**: Stock reservado no implementado

---

## 1. MÓDULO: CLIENTES (TERCEROS)

### 1.1 Backend - Estado Actual

#### 1.1.1 Controllers
| Endpoint | Método | Implementado | Funcionalidad | Legacy Equivalente |
|---|---|---|---|---|
| `/api/terceros` | GET | ✅ | Listado paginado con filtros | `CLIENTES_Listado.asp` |
| `/api/terceros/{id}` | GET | ✅ | Detalle completo | `FichaDeCliente.asp` |
| `/api/terceros/legajo/{legajo}` | GET | ✅ | Búsqueda por legajo | `ConsultarClientes_Listado.asp` |
| `/api/terceros/documento/{nroDocumento}` | GET | ✅ | Búsqueda por documento | `ConsultarClientes_Listado.asp` |
| `/api/terceros/clientes/selector-ventas` | GET | ✅ | Selector optimizado ventas | Combo clientes en pedidos |
| `/api/terceros/{id}/perfil-comercial` | GET | ✅ | Perfil comercial ampliado | Pestaña comercial FichaCliente |
| `/api/terceros/{id}/cuenta-corriente` | GET | ✅ | Configuración cuenta corriente | Pestaña cuenta corriente |
| `/api/terceros/{id}/domicilios` | GET | ✅ | Domicilios del tercero | Pestaña domicilios |
| `/api/terceros/{id}/sucursales-entrega` | GET | ✅ | Sucursales de entrega | Sucursales de entrega |
| `/api/terceros/{id}/contactos` | GET | ✅ | Contactos del tercero | Contactos |
| `/api/terceros/{id}/transportes` | GET | ✅ | Transportes habituales | Transportes |

#### 1.1.2 Queries Implementadas
| Query | Handler | DTO Response | Tests | Notas |
|---|---|---|---|---|
| `GetTercerosPagedQuery` | ✅ | `TerceroListDto` | ✅ | Filtros completos |
| `GetTerceroByIdQuery` | ✅ | `TerceroDto` | ✅ | Detalle completo |
| `GetTerceroByLegajoQuery` | ✅ | `TerceroDto` | ⚠️ | Falta test |
| `GetTerceroByNroDocumentoQuery` | ✅ | `TerceroDto` | ✅ | Nuevo, completo |
| `GetClientesSelectorVentasQuery` | ✅ | `ClienteSelectorVentasDto` | ✅ | Nuevo, optimizado |
| `GetTerceroPerfilComercialQuery` | ✅ | `TerceroPerfilComercialDto` | ⚠️ | Falta test |
| `GetTerceroCuentaCorrienteQuery` | ✅ | `TerceroCuentaCorrienteDto` | ⚠️ | Falta test |
| `GetTerceroDomiciliosQuery` | ✅ | `TerceroDomicilioDto[]` | ❌ | Sin tests |
| `GetTerceroSucursalesEntregaQuery` | ✅ | `TerceroSucursalEntregaDto[]` | ❌ | Sin tests |
| `GetTerceroContactosQuery` | ✅ | `TerceroContactoDto[]` | ❌ | Sin tests |
| `GetTerceroTransportesQuery` | ✅ | `TerceroTransporteDto[]` | ❌ | Sin tests |

#### 1.1.3 DTOs Principales
```csharp
// IMPLEMENTADOS
TerceroListDto {
    Id, Legajo, RazonSocial, NombreFantasia, NroDocumento,
    TipoDocumento, CondicionIvaId, EsCliente, EsProveedor, 
    EsEmpleado, Activo, FechaAlta, Saldo
}

ClienteSelectorVentasDto {
    Id, Legajo, RazonSocial, NombreFantasia, NroDocumento,
    CondicionIvaDescripcion, Bloqueado, MotivoBloqueo,
    TieneLimiteCredito, SaldoDisponible, ListaPreciosId,
    ListaPreciosDescripcion
}

TerceroDto {
    // Completo: identidad, fiscal, comercial, financiero
    Id, Legajo, RazonSocial, NombreFantasia, TipoPersona,
    NroDocumento, TipoDocumento, CondicionIvaId,
    EsCliente, EsProveedor, EsEmpleado, Activo,
    Domicilios, Contactos, SucursalesEntrega, Transportes,
    PerfilComercial, CuentaCorriente, ...
}

TerceroPerfilComercialDto {
    ListaPreciosId, VendedorId, CanalVentaId, CondicionPagoId,
    PlazoDias, DiasVencimiento[], DescuentoMaximo,
    TieneDescuentoPersonalizado, Observaciones
}

TerceroCuentaCorrienteDto {
    TieneLimiteCredito, LimiteCredito, MonedaLimiteCreditoId,
    SaldoActual, SaldoDisponible, Bloqueado, MotivoBloqueo,
    RequiereAutorizacionCredito, CantidadChequesDiferidosPermitidos
}
```

#### 1.1.4 Validaciones de Negocio Implementadas
| Validación | Implementada | Ubicación | Notas |
|---|---|---|---|
| Cliente activo | ✅ | `TerceroOperacionValidationService` | OK |
| Cliente bloqueado | ✅ | `ClienteSelectorVentasQuery` | OK |
| Límite de crédito | ⚠️ | `TerceroCuentaCorrienteDto` | Solo cálculo, falta validación en ventas |
| Cliente puede facturar (IVA) | ✅ | `TerceroDto.EsClienteFacturable()` | OK |
| Cliente tiene sucursal entrega | ⚠️ | - | No validado |
| Validación de saldo | ⚠️ | - | Cálculo OK, validación falta |

### 1.2 Comparación con Legacy

#### 1.2.1 Campos Visibles en Legado (CLIENTES_Listado.asp)
| Campo Legacy | Backend Actual | DTO | Gap |
|---|---|---|---|
| Legajo | ✅ `Legajo` | `TerceroListDto` | ✅ OK |
| Razón Social | ✅ `RazonSocial` | `TerceroListDto` | ✅ OK |
| Nombre Fantasía | ✅ `NombreFantasia` | `TerceroListDto` | ✅ OK |
| CUIT/DNI | ✅ `NroDocumento` | `TerceroListDto` | ✅ OK |
| Condición IVA | ✅ `CondicionIvaId` | `TerceroListDto` | ✅ OK |
| Activo | ✅ `Activo` | `TerceroListDto` | ✅ OK |
| Saldo | ✅ `Saldo` | `TerceroListDto` | ✅ OK |
| Domicilio principal | ❌ | - | ⚠️ **GAP** - No expuesto en list |
| Teléfono principal | ❌ | - | ⚠️ **GAP** - No expuesto en list |
| Email principal | ❌ | - | ⚠️ **GAP** - No expuesto en list |
| Categoría cliente | ✅ | `TerceroListDto` (filter) | ⚠️ No en DTO list |
| Estado cliente | ✅ | `TerceroListDto` (filter) | ⚠️ No en DTO list |
| Vendedor asignado | ❌ | - | ⚠️ **GAP** - No expuesto en list |
| Lista de precios | ❌ | - | ⚠️ **GAP** - No expuesto en list |

#### 1.2.2 Filtros Disponibles en Legado
| Filtro Legacy | Backend Actual | Gap |
|---|---|---|
| Búsqueda texto (razón social, legajo, CUIT) | ✅ `search` | ✅ OK |
| Solo clientes | ✅ `soloClientes` | ✅ OK |
| Solo proveedores | ✅ `soloProveedores` | ✅ OK |
| Solo activos | ✅ `soloActivos` | ✅ OK |
| Condición IVA | ✅ `condicionIvaId` | ✅ OK |
| Categoría cliente | ✅ `categoriaClienteId` | ✅ OK |
| Estado cliente | ✅ `estadoClienteId` | ✅ OK |
| Vendedor | ❌ | ⚠️ **GAP** |
| Zona geográfica | ❌ | ⚠️ **GAP** |
| Sucursal | ✅ `sucursalId` | ✅ OK |

#### 1.2.3 Funcionalidad del Selector de Clientes en Ventas (Legacy)
| Funcionalidad | Backend Actual | Gap |
|---|---|---|
| Búsqueda rápida (texto libre) | ✅ `GetClientesSelectorVentasQuery.search` | ✅ OK |
| Filtro por sucursal | ✅ `sucursalId` | ✅ OK |
| Solo clientes activos | ✅ Implementado | ✅ OK |
| Solo clientes no bloqueados | ✅ Implementado | ✅ OK |
| Información de bloqueo visible | ✅ `Bloqueado`, `MotivoBloqueo` | ✅ OK |
| Límite crédito y saldo | ✅ `TieneLimiteCredito`, `SaldoDisponible` | ✅ OK |
| Lista de precios asignada | ✅ `ListaPreciosId`, `ListaPreciosDescripcion` | ✅ OK |
| Condición de pago visible | ❌ | ⚠️ **GAP** |
| Vendedor asignado visible | ❌ | ⚠️ **GAP** |
| Canal de venta visible | ❌ | ⚠️ **GAP** |
| Categoría cliente visible | ❌ | ⚠️ **GAP** |
| Plazo días visible | ❌ | ⚠️ **GAP** |
| Descuento máximo visible | ❌ | ⚠️ **GAP** |

### 1.3 Gaps Identificados - CLIENTES

#### 1.3.1 Gaps Críticos (Alta Prioridad)
| # | Gap | Impacto | Módulo Afectado | Esfuerzo | Acción Recomendada |
|---|---|---|---|---|---|
| C-1 | Domicilio, teléfono y email NO expuestos en `TerceroListDto` | ALTO | Listado clientes, selector | BAJO | Agregar a DTO |
| C-2 | Vendedor asignado NO visible en selector ventas | MEDIO | Pedidos, remitos, facturas | BAJO | Agregar a `ClienteSelectorVentasDto` |
| C-3 | Condición de pago NO visible en selector ventas | MEDIO | Pedidos, remitos, facturas | BAJO | Agregar a `ClienteSelectorVentasDto` |
| C-4 | Plazo días NO visible en selector ventas | MEDIO | Pedidos, remitos, facturas | BAJO | Agregar a `ClienteSelectorVentasDto` |
| C-5 | Validación límite crédito NO ejecutada en venta | ALTO | Pedidos, remitos, facturas | MEDIO | Agregar validación en commands |
| C-6 | Validación cliente bloqueado NO ejecutada | ALTO | Pedidos, remitos, facturas | BAJO | Agregar validación en commands |

#### 1.3.2 Gaps Medios (Media Prioridad)
| # | Gap | Impacto | Módulo Afectado | Esfuerzo | Acción Recomendada |
|---|---|---|---|---|---|
| C-7 | Filtro por vendedor en listado | MEDIO | Listado clientes | BAJO | Agregar filtro query |
| C-8 | Filtro por zona geográfica en listado | BAJO | Listado clientes | BAJO | Agregar filtro query |
| C-9 | Categoría cliente NO visible en listado | BAJO | Listado clientes | BAJO | Agregar a DTO |
| C-10 | Estado cliente NO visible en listado | BAJO | Listado clientes | BAJO | Agregar a DTO |
| C-11 | Descuento máximo NO visible en selector | MEDIO | Pedidos, remitos, facturas | BAJO | Agregar a DTO |
| C-12 | Tests faltantes para queries comerciales | MEDIO | Testing | MEDIO | Agregar tests unitarios |

#### 1.3.3 Gaps Bajos (Baja Prioridad)
| # | Gap | Impacto | Módulo Afectado | Esfuerzo | Acción Recomendada |
|---|---|---|---|---|---|
| C-13 | Canal venta NO visible en selector | BAJO | Pedidos, remitos | BAJO | Agregar a DTO |
| C-14 | Historial comercial NO implementado | BAJO | Análisis | ALTO | Postergar |
| C-15 | Estadísticas de ventas NO expuestas | BAJO | Análisis | ALTO | Postergar |

### 1.4 Recomendaciones - CLIENTES

#### Acciones Inmediatas (Sprint 1)
1. ✅ **Ampliar `ClienteSelectorVentasDto`** con: vendedor, condición pago, plazo días, descuento máximo
2. ✅ **Agregar validaciones en commands** de ventas: cliente bloqueado, límite crédito
3. ✅ **Ampliar `TerceroListDto`** con: domicilio, teléfono, email (para grillas)
4. ✅ **Agregar tests unitarios** para `GetClientesSelectorVentasQuery` y validaciones

#### Acciones Corto Plazo (Sprint 2)
1. Agregar filtro por vendedor y zona en `GetTercerosPagedQuery`
2. Exponer categoría y estado cliente en `TerceroListDto`
3. Completar tests de queries comerciales

---

## 2. MÓDULO: PRODUCTOS (ITEMS)

### 2.1 Backend - Estado Actual

#### 2.1.1 Controllers
| Endpoint | Método | Implementado | Funcionalidad | Legacy Equivalente |
|---|---|---|---|---|
| `/api/items` | GET | ✅ | Listado paginado con filtros | `ITEMSNOFINANCIEROS_Listado.asp` |
| `/api/items/{id}` | GET | ✅ | Detalle completo | `ITEMS_EditarForm.asp` |
| `/api/items/por-codigo/{codigo}` | GET | ✅ | Búsqueda por código | Búsqueda código |
| `/api/items/por-codigo-alternativo/{codigo}` | GET | ✅ | Búsqueda código alternativo | Búsqueda código alternativo |
| `/api/items/por-codigo-barras/{codigo}` | GET | ✅ | Búsqueda código barras | Búsqueda código barras |
| `/api/items/vendibles` | GET | ✅ | Selector optimizado ventas | Combo items en pedidos |
| `/api/items/{id}/precio` | GET | ✅ | Precio resuelto (lista/cliente) | Cálculo precio |
| `/api/items/{id}/stock` | GET | ✅ | Stock por depósito | Consulta stock |

#### 2.1.2 Queries Implementadas
| Query | Handler | DTO Response | Tests | Notas |
|---|---|---|---|---|
| `GetItemsPagedQuery` | ✅ | `ItemListDto` | ✅ | Filtros completos |
| `GetItemByIdQuery` | ✅ | `ItemDto` | ⚠️ | Falta test completo |
| `GetItemsVendiblesQuery` | ✅ | `ItemSelectorDto` | ✅ | Nuevo, optimizado |
| `GetItemPrecioQuery` | ✅ | `ItemPrecioDto` | ⚠️ | Falta test con lista precios |
| `GetStockByItemQuery` | ✅ | `StockItemDto[]` | ❌ | Sin tests |

#### 2.1.3 DTOs Principales
```csharp
// IMPLEMENTADOS
ItemListDto {
    Id, Codigo, CodigoAlternativo, CodigoBarras, Descripcion,
    CategoriaId, CategoriaDescripcion, UnidadMedidaId,
    UnidadMedidaDescripcion, EsProducto, EsServicio, EsFinanciero,
    AplicaVentas, AplicaCompras, Activo, PrecioVenta, MonedaId,
    StockActual
}

ItemSelectorDto {
    Id, Codigo, CodigoAlternativo, CodigoBarras, Descripcion,
    UnidadMedidaDescripcion, PrecioVenta, MonedaId,
    StockDisponible, TieneStock
}

ItemDto {
    // Completo: identidad, precios, stock, atributos, configuración
    Id, Codigo, CodigoAlternativo, CodigoBarras, Descripcion,
    DescripcionLarga, CategoriaId, UnidadMedidaId, MarcaId,
    EsProducto, EsServicio, EsFinanciero, AplicaVentas, AplicaCompras,
    Activo, PrecioVenta, PrecioCompra, MonedaId, StockMinimo,
    StockActual, Atributos[], ...
}

ItemPrecioDto {
    ItemId, PrecioBase, PrecioFinal, MonedaId,
    DescuentoPorcentaje, DescuentoImporte, ListaPreciosAplicada,
    FechaVigencia, EsPersonalizado
}

StockItemDto {
    ItemId, DepositoId, DepositoDescripcion, Cantidad,
    StockReservado, StockDisponible, UpdatedAt
}
```

#### 2.1.4 Validaciones de Negocio Implementadas
| Validación | Implementada | Ubicación | Notas |
|---|---|---|---|
| Item activo | ✅ | `GetItemsVendiblesQuery` | OK |
| Item vendible (AplicaVentas) | ✅ | `GetItemsVendiblesQuery` | OK |
| Item no financiero | ✅ | `GetItemsVendiblesQuery` | OK |
| Item es producto o servicio | ✅ | `GetItemsVendiblesQuery` | OK |
| Stock disponible | ⚠️ | `StockItemDto` | Cálculo OK, validación falta |
| Stock reservado | ❌ | - | **NO IMPLEMENTADO** |
| Precio vigente | ✅ | `GetItemPrecioQuery` | OK |
| Precio por lista cliente | ✅ | `GetItemPrecioQuery` | OK |

### 2.2 Comparación con Legacy

#### 2.2.1 Campos Visibles en Legado (ITEMSNOFINANCIEROS_Listado.asp)
| Campo Legacy | Backend Actual | DTO | Gap |
|---|---|---|---|
| Código | ✅ `Codigo` | `ItemListDto` | ✅ OK |
| Código alternativo | ✅ `CodigoAlternativo` | `ItemListDto` | ✅ OK |
| Código de barras | ✅ `CodigoBarras` | `ItemListDto` | ✅ OK |
| Descripción | ✅ `Descripcion` | `ItemListDto` | ✅ OK |
| Categoría | ✅ `CategoriaDescripcion` | `ItemListDto` | ✅ OK |
| Unidad medida | ✅ `UnidadMedidaDescripcion` | `ItemListDto` | ✅ OK |
| Activo | ✅ `Activo` | `ItemListDto` | ✅ OK |
| Precio venta | ✅ `PrecioVenta` | `ItemListDto` | ✅ OK |
| Moneda | ✅ `MonedaId` | `ItemListDto` | ⚠️ Falta descripción |
| Stock actual | ✅ `StockActual` | `ItemListDto` | ✅ OK |
| Stock mínimo | ❌ | - | ⚠️ **GAP** - No expuesto en list |
| Stock reservado | ❌ | - | ⚠️ **GAP** - No expuesto en list |
| Stock disponible | ❌ | - | ⚠️ **GAP** - No expuesto en list |
| Marca | ❌ | - | ⚠️ **GAP** - No expuesto en list |
| Aplica ventas | ✅ `AplicaVentas` | `ItemListDto` | ✅ OK |

#### 2.2.2 Filtros Disponibles en Legado
| Filtro Legacy | Backend Actual | Gap |
|---|---|---|
| Búsqueda texto (descripción, código) | ✅ `search` | ✅ OK |
| Solo activos | ✅ `soloActivos` | ✅ OK |
| Solo con stock | ✅ `soloConStock` | ✅ OK |
| Solo productos | ✅ `soloProductos` | ✅ OK |
| Solo servicios | ✅ `soloServicios` | ✅ OK |
| Solo vendibles | ✅ `soloVendibles` | ✅ OK |
| Categoría | ✅ `categoriaId` | ✅ OK |
| Marca | ❌ | ⚠️ **GAP** |
| Rango de precio | ❌ | ⚠️ **GAP** |

#### 2.2.3 Funcionalidad del Selector de Productos en Ventas (Legacy)
| Funcionalidad | Backend Actual | Gap |
|---|---|---|
| Búsqueda rápida (texto libre) | ✅ `GetItemsVendiblesQuery.search` | ✅ OK |
| Filtro solo con stock | ✅ `soloConStock` | ✅ OK |
| Solo items vendibles | ✅ Implementado | ✅ OK |
| Búsqueda por código exacto | ✅ `/por-codigo/{codigo}` | ✅ OK |
| Búsqueda por código alternativo | ✅ `/por-codigo-alternativo/{codigo}` | ✅ OK |
| Búsqueda por código barras | ✅ `/por-codigo-barras/{codigo}` | ✅ OK |
| Precio visible en selector | ✅ `PrecioVenta` en `ItemSelectorDto` | ✅ OK |
| Stock visible en selector | ✅ `StockDisponible` en `ItemSelectorDto` | ✅ OK |
| Unidad medida visible | ✅ `UnidadMedidaDescripcion` | ✅ OK |
| Marca visible | ❌ | ⚠️ **GAP** |
| Categoría visible | ❌ | ⚠️ **GAP** |
| Stock reservado considerado | ❌ | ⚠️ **GAP CRÍTICO** |

### 2.3 Gaps Identificados - PRODUCTOS

#### 2.3.1 Gaps Críticos (Alta Prioridad)
| # | Gap | Impacto | Módulo Afectado | Esfuerzo | Acción Recomendada |
|---|---|---|---|---|---|
| P-1 | Stock reservado NO implementado | **CRÍTICO** | Pedidos, disponibilidad | ALTO | Implementar en Domain + Service |
| P-2 | Stock disponible NO calculado correctamente | **CRÍTICO** | Selector vendibles, validación | MEDIO | Ajustar cálculo: `Stock - Reservado` |
| P-3 | Moneda descripción NO expuesta en listado | MEDIO | Listado productos | BAJO | Agregar a DTO |
| P-4 | Validación stock en venta NO ejecutada | ALTO | Pedidos, remitos | MEDIO | Agregar validación en commands |

#### 2.3.2 Gaps Medios (Media Prioridad)
| # | Gap | Impacto | Módulo Afectado | Esfuerzo | Acción Recomendada |
|---|---|---|---|---|---|
| P-5 | Stock mínimo NO visible en listado | BAJO | Listado productos | BAJO | Agregar a DTO |
| P-6 | Stock reservado NO visible en listado | MEDIO | Listado productos | BAJO | Agregar a DTO |
| P-7 | Stock disponible NO visible en listado | MEDIO | Listado productos | BAJO | Agregar a DTO |
| P-8 | Marca NO visible en listado | BAJO | Listado productos | BAJO | Agregar a DTO |
| P-9 | Filtro por marca en listado | BAJO | Listado productos | BAJO | Agregar filtro query |
| P-10 | Filtro por rango precio en listado | BAJO | Listado productos | MEDIO | Agregar filtro query |
| P-11 | Marca NO visible en selector ventas | BAJO | Selector ventas | BAJO | Agregar a `ItemSelectorDto` |
| P-12 | Categoría NO visible en selector ventas | BAJO | Selector ventas | BAJO | Agregar a `ItemSelectorDto` |
| P-13 | Tests faltantes para stock y precio | MEDIO | Testing | MEDIO | Agregar tests unitarios |

#### 2.3.3 Gaps Bajos (Baja Prioridad)
| # | Gap | Impacto | Módulo Afectado | Esfuerzo | Acción Recomendada |
|---|---|---|---|---|---|
| P-14 | Historial de precios NO expuesto | BAJO | Análisis | MEDIO | Postergar |
| P-15 | Movimientos stock NO expuestos | BAJO | Análisis | MEDIO | Postergar |
| P-16 | Componentes ítem NO expuestos | BAJO | Producción | ALTO | Postergar |

### 2.4 Recomendaciones - PRODUCTOS

#### Acciones Inmediatas (Sprint 1)
1. 🔴 **CRÍTICO: Implementar stock reservado** en Domain (`StockItem.StockReservado`)
2. 🔴 **CRÍTICO: Ajustar cálculo stock disponible** = `Stock - Reservado`
3. ✅ **Agregar validación stock** en commands de ventas
4. ✅ **Ampliar `ItemListDto`** con: stock mínimo, reservado, disponible, marca
5. ✅ **Agregar tests unitarios** para stock y precio

#### Acciones Corto Plazo (Sprint 2)
1. Agregar filtro por marca y rango precio en `GetItemsPagedQuery`
2. Ampliar `ItemSelectorDto` con marca y categoría
3. Completar tests de queries de stock

---

## 3. MÓDULO: PEDIDOS

### 3.1 Backend - Estado Actual

#### 3.1.1 Controllers
| Endpoint | Método | Implementado | Funcionalidad | Legacy Equivalente |
|---|---|---|---|---|
| `/api/ventas/notas-pedido` | POST | ✅ | Crear nota pedido borrador | Crear pedido |
| `/api/ventas/pedidos` | GET | ✅ | Listado paginado pedidos | Listado pedidos |
| `/api/ventas/documentos/{id}` | GET | ✅ | Detalle pedido | Ver pedido |
| `/api/ventas/documentos/{id}/emitir` | POST | ✅ | Emitir pedido | Confirmar pedido |
| `/api/ventas/documentos/{id}/convertir` | POST | ✅ | Convertir a remito/factura | Convertir pedido |
| `/api/ventas/pedidos/cerrar-masivo` | POST | ✅ | Cerrar pedidos masivo | Cerrar pedidos |
| `/api/ventas/documentos/{id}/vinculos` | GET | ✅ | Ver vinculaciones | Ver seguimiento |

#### 3.1.2 Commands Implementados
| Command | Handler | Validaciones | Tests | Notas |
|---|---|---|---|---|
| `CrearBorradorVentaCommand` | ✅ | ⚠️ Parcial | ⚠️ Básico | Crea pedido borrador |
| `EmitirDocumentoVentaCommand` | ✅ | ⚠️ Parcial | ❌ | Emite pedido |
| `ConvertirDocumentoVentaCommand` | ✅ | ⚠️ Parcial | ❌ | Convierte a remito/factura |
| `CerrarPedidoCommand` | ✅ | ⚠️ Parcial | ❌ | Cierra pedido individual |
| `CerrarPedidosMasivoCommand` | ✅ | ⚠️ Parcial | ❌ | Cierra múltiples pedidos |
| `ActualizarCumplimientoPedidoCommand` | ✅ | ⚠️ Parcial | ❌ | Actualiza % cumplimiento |
| `VincularComprobanteVentaCommand` | ✅ | ⚠️ Parcial | ❌ | Vincula comprobantes |

#### 3.1.3 Queries Implementados
| Query | Handler | DTO Response | Tests | Notas |
|---|---|---|---|---|
| `GetPedidosConEstadoQuery` | ✅ | `PedidoConEstadoDto` | ❌ | Lista pedidos filtrados |
| `GetPedidoVinculacionesQuery` | ✅ | `PedidoVinculacionesDto` | ❌ | Seguimiento vinculaciones |
| `GetComprobanteDetalleQuery` | ✅ | `ComprobanteDetalleDto` | ⚠️ | Detalle pedido |

#### 3.1.4 Estados y Transiciones
```csharp
// ESTADOS IMPLEMENTADOS
EstadoComprobante {
    Borrador,      // ✅ Implementado
    Emitido,       // ✅ Implementado
    Anulado,       // ✅ Implementado
    ConError       // ✅ Implementado
}

EstadoPedido {
    Pendiente,           // ✅ Implementado
    EnPreparacion,       // ⚠️ Parcial
    ParcialmenteRemitido,// ⚠️ Parcial
    TotalmenteRemitido,  // ⚠️ Parcial
    Cerrado,             // ✅ Implementado
    Anulado              // ✅ Implementado
}

// WORKFLOW IMPLEMENTADO
Borrador → Emitido → (Preparación → Remitido) → Cerrado
                 \→ Anulado
```

### 3.2 Comparación con Legacy

#### 3.2.1 Funcionalidad Legacy de Pedidos
| Funcionalidad | Backend Actual | Gap |
|---|---|---|
| Crear pedido borrador | ✅ | ✅ OK |
| Confirmar pedido | ✅ | ✅ OK |
| Modificar pedido emitido | ⚠️ | ⚠️ **GAP** - No permitido |
| Anular pedido | ✅ | ✅ OK |
| Agregar/quitar ítems | ✅ | ✅ OK (solo borrador) |
| Cambiar cantidades | ✅ | ✅ OK (solo borrador) |
| Cambiar precios | ✅ | ✅ OK (solo borrador) |
| Aplicar descuentos | ✅ | ✅ OK |
| Cambiar fecha entrega | ⚠️ | ⚠️ **GAP** - Falta endpoint |
| Asignar transporte | ⚠️ | ⚠️ **GAP** - Falta campo |
| Asignar depósito origen | ⚠️ | ⚠️ **GAP** - Falta campo |
| Observaciones comerciales | ✅ | ✅ OK |
| Ver remitos generados | ✅ | ✅ OK |
| Ver facturas generadas | ✅ | ✅ OK |
| Cerrar pedido parcial | ⚠️ | ⚠️ **GAP** - Validación incompleta |
| Cerrar pedido total | ✅ | ✅ OK |
| Reabrir pedido cerrado | ❌ | ⚠️ **GAP** |

#### 3.2.2 Validaciones Legacy vs Actual
| Validación | Legacy | Backend Actual | Gap |
|---|---|---|---|
| Cliente activo | ✅ | ⚠️ | ⚠️ No ejecutada |
| Cliente no bloqueado | ✅ | ⚠️ | ⚠️ No ejecutada |
| Cliente con límite crédito | ✅ | ❌ | ⚠️ **GAP** |
| Ítems activos | ✅ | ⚠️ | ⚠️ No ejecutada |
| Ítems vendibles | ✅ | ⚠️ | ⚠️ No ejecutada |
| Stock disponible | ✅ | ❌ | ⚠️ **GAP CRÍTICO** |
| Precio vigente | ✅ | ⚠️ | ⚠️ No validado |
| Descuento máximo cliente | ✅ | ⚠️ | ⚠️ No validado |
| Fecha entrega >= hoy | ✅ | ❌ | ⚠️ **GAP** |
| Cantidad > 0 | ✅ | ✅ | ✅ OK |
| Total pedido >= mínimo | ✅ | ❌ | ⚠️ **GAP** |

### 3.3 Gaps Identificados - PEDIDOS

#### 3.3.1 Gaps Críticos (Alta Prioridad)
| # | Gap | Impacto | Módulo Afectado | Esfuerzo | Acción Recomendada |
|---|---|---|---|---|---|
| PD-1 | Validación stock disponible NO ejecutada | **CRÍTICO** | Crear/emitir pedido | MEDIO | Agregar validación |
| PD-2 | Validación límite crédito NO ejecutada | **CRÍTICO** | Crear/emitir pedido | MEDIO | Agregar validación |
| PD-3 | Validación cliente bloqueado NO ejecutada | ALTO | Crear/emitir pedido | BAJO | Agregar validación |
| PD-4 | Cambio fecha entrega NO permitido | ALTO | Modificar pedido | MEDIO | Agregar comando |
| PD-5 | Depósito origen NO asignado | ALTO | Preparación pedido | MEDIO | Agregar campo + lógica |
| PD-6 | Transporte NO asignado | MEDIO | Preparación pedido | MEDIO | Agregar campo + lógica |

#### 3.3.2 Gaps Medios (Media Prioridad)
| # | Gap | Impacto | Módulo Afectado | Esfuerzo | Acción Recomendada |
|---|---|---|---|---|---|
| PD-7 | Modificar pedido emitido NO permitido | MEDIO | Workflow pedido | ALTO | Evaluar casos de uso |
| PD-8 | Reabrir pedido cerrado NO permitido | MEDIO | Workflow pedido | MEDIO | Agregar comando |
| PD-9 | Validación descuento máximo NO ejecutada | MEDIO | Crear pedido | BAJO | Agregar validación |
| PD-10 | Validación fecha entrega NO ejecutada | MEDIO | Crear pedido | BAJO | Agregar validación |
| PD-11 | Validación total mínimo NO ejecutada | BAJO | Crear pedido | BAJO | Agregar validación |
| PD-12 | Validación ítems activos NO ejecutada | MEDIO | Crear pedido | BAJO | Agregar validación |
| PD-13 | Tests faltantes para workflow | ALTO | Testing | ALTO | Agregar tests |

#### 3.3.3 Gaps Bajos (Baja Prioridad)
| # | Gap | Impacto | Módulo Afectado | Esfuerzo | Acción Recomendada |
|---|---|---|---|---|---|
| PD-14 | Historial cambios pedido NO expuesto | BAJO | Auditoría | MEDIO | Postergar |
| PD-15 | Notificaciones pedido NO implementadas | BAJO | Comunicaciones | ALTO | Postergar |

### 3.4 Recomendaciones - PEDIDOS

#### Acciones Inmediatas (Sprint 1)
1. 🔴 **CRÍTICO: Agregar validación stock** en `CrearBorradorVentaCommand` y `EmitirDocumentoVentaCommand`
2. 🔴 **CRÍTICO: Agregar validación límite crédito** en `EmitirDocumentoVentaCommand`
3. ✅ **Agregar validación cliente bloqueado** en `CrearBorradorVentaCommand`
4. ✅ **Agregar campo depósito origen** en `Comprobante` y lógica de asignación
5. ✅ **Agregar comando cambio fecha entrega** para pedidos emitidos
6. ✅ **Agregar tests unitarios** para validaciones y workflow

#### Acciones Corto Plazo (Sprint 2)
1. Evaluar e implementar modificación de pedido emitido (casos de uso específicos)
2. Implementar comando reabrir pedido cerrado
3. Completar validaciones de descuento y total mínimo
4. Completar tests de workflow completo

---

## 4. MÓDULO: REMITOS

### 4.1 Backend - Estado Actual

#### 4.1.1 Controllers
| Endpoint | Método | Implementado | Funcionalidad | Legacy Equivalente |
|---|---|---|---|---|
| `/api/ventas/remitos` | POST | ✅ | Crear remito borrador | Crear remito |
| `/api/ventas/remitos` | GET | ✅ | Listado remitos | Listado remitos |
| `/api/ventas/remitos/{id}` | GET | ✅ | Detalle remito | Ver remito |
| `/api/ventas/remitos/{id}/emitir` | POST | ✅ | Emitir remito | Confirmar remito |
| `/api/ventas/remitos/{id}/cot` | PUT | ✅ | Asignar COT | Asignar COT |
| `/api/comprobantes/{id}/remito/atributos` | PUT | ✅ | Asignar atributos remito | Atributos remito |
| `/api/ventas/remitos/emitir-masivo` | POST | ✅ | Emitir remitos masivo | Emitir masivo |

#### 4.1.2 Commands Implementados
| Command | Handler | Validaciones | Tests | Notas |
|---|---|---|---|---|
| `CrearBorradorVentaCommand` | ✅ | ⚠️ Parcial | ⚠️ Básico | Crea remito borrador |
| `EmitirDocumentoVentaCommand` | ✅ | ⚠️ Parcial | ❌ | Emite remito |
| `UpsertRemitoCotCommand` | ✅ | ✅ | ❌ | Asigna COT |
| `ReplaceRemitoAtributosCommand` | ✅ | ✅ | ❌ | Asigna atributos |
| `EmitirRemitosVentaMasivosCommand` | ✅ | ⚠️ Parcial | ❌ | Emite masivo |
| `ConvertirDocumentoVentaCommand` | ✅ | ⚠️ Parcial | ❌ | Convierte a factura |

#### 4.1.3 Queries Implementados
| Query | Handler | DTO Response | Tests | Notas |
|---|---|---|---|---|
| `GetRemitosPagedQuery` | ✅ | `ComprobanteListDto` | ❌ | Lista remitos filtrados |
| `GetComprobanteDetalleQuery` | ✅ | `ComprobanteDetalleDto` | ⚠️ | Detalle remito |

#### 4.1.4 Atributos Remito (COT y Complementarios)
```csharp
// ATRIBUTOS IMPLEMENTADOS
RemitoCot {
    Numero,              // ✅ Implementado
    FechaVigencia,       // ✅ Implementado
    Descripcion          // ✅ Implementado
}

// ATRIBUTOS COMPLEMENTARIOS (ReplaceRemitoAtributosCommand)
RemitoAtributo {
    Clave,               // ✅ Implementado (flexible)
    Valor,               // ✅ Implementado
    TipoDato             // ✅ Implementado (String/Int/Date/Decimal)
}

// ESTRUCTURA LEGACY ESPERADA
Atributos COT legacy {
    ConductorNombre,     // ⚠️ No tipado
    ConductorDocumento,  // ⚠️ No tipado
    TransporteNombre,    // ⚠️ No tipado
    TransporteCUIT,      // ⚠️ No tipado
    VehiculoPatente,     // ⚠️ No tipado
    VehiculoDominio,     // ⚠️ No tipado
    RemitenteNombre,     // ⚠️ No tipado
    DestinatarioNombre,  // ⚠️ No tipado
    LugarOrigen,         // ⚠️ No tipado
    LugarDestino         // ⚠️ No tipado
}
```

### 4.2 Comparación con Legacy

#### 4.2.1 Funcionalidad Legacy de Remitos
| Funcionalidad | Backend Actual | Gap |
|---|---|---|---|
| Crear remito directo | ✅ | ✅ OK |
| Crear remito desde pedido | ✅ | ✅ OK |
| Emitir remito | ✅ | ✅ OK |
| Anular remito | ✅ | ✅ OK |
| Asignar COT | ✅ | ✅ OK |
| Asignar conductor | ✅ (atributo) | ⚠️ No tipado |
| Asignar transporte | ✅ (atributo) | ⚠️ No tipado |
| Asignar vehículo | ✅ (atributo) | ⚠️ No tipado |
| Asignar origen/destino | ✅ (atributo) | ⚠️ No tipado |
| Remito valorizado | ✅ | ✅ OK |
| Remito no valorizado | ✅ | ✅ OK |
| Remito afecta stock | ✅ | ✅ OK |
| Remito no afecta stock | ✅ | ✅ OK |
| Convertir a factura | ✅ | ✅ OK |
| Ver factura generada | ✅ | ✅ OK |
| Imprimir remito | ⚠️ | ⚠️ **GAP** |

#### 4.2.2 Validaciones Legacy vs Actual
| Validación | Legacy | Backend Actual | Gap |
|---|---|---|---|
| Cliente activo | ✅ | ⚠️ | ⚠️ No ejecutada |
| Cliente no bloqueado | ✅ | ⚠️ | ⚠️ No ejecutada |
| Ítems activos | ✅ | ⚠️ | ⚠️ No ejecutada |
| Ítems vendibles | ✅ | ⚠️ | ⚠️ No ejecutada |
| Stock disponible (si afecta stock) | ✅ | ❌ | ⚠️ **GAP CRÍTICO** |
| COT obligatorio (según config) | ✅ | ❌ | ⚠️ **GAP** |
| Atributos obligatorios (según config) | ✅ | ❌ | ⚠️ **GAP** |
| Cantidad > 0 | ✅ | ✅ | ✅ OK |
| Precio > 0 (si valorizado) | ✅ | ⚠️ | ⚠️ No validado |

### 4.3 Gaps Identificados - REMITOS

#### 4.3.1 Gaps Críticos (Alta Prioridad)
| # | Gap | Impacto | Módulo Afectado | Esfuerzo | Acción Recomendada |
|---|---|---|---|---|---|
| R-1 | Validación stock disponible NO ejecutada | **CRÍTICO** | Emitir remito | MEDIO | Agregar validación |
| R-2 | Atributos COT NO tipados | ALTO | Asignar atributos | MEDIO | Crear estructura tipada |
| R-3 | Validación COT obligatorio NO ejecutada | ALTO | Emitir remito | BAJO | Agregar validación |
| R-4 | Validación precio valorizado NO ejecutada | MEDIO | Emitir remito | BAJO | Agregar validación |

#### 4.3.2 Gaps Medios (Media Prioridad)
| # | Gap | Impacto | Módulo Afectado | Esfuerzo | Acción Recomendada |
|---|---|---|---|---|---|
| R-5 | Validación cliente bloqueado NO ejecutada | MEDIO | Crear remito | BAJO | Agregar validación |
| R-6 | Validación ítems activos NO ejecutada | MEDIO | Crear remito | BAJO | Agregar validación |
| R-7 | Impresión remito NO implementada | MEDIO | Imprimir | ALTO | Implementar servicio |
| R-8 | Tests faltantes para atributos y COT | MEDIO | Testing | MEDIO | Agregar tests |

#### 4.3.3 Gaps Bajos (Baja Prioridad)
| # | Gap | Impacto | Módulo Afectado | Esfuerzo | Acción Recomendada |
|---|---|---|---|---|---|
| R-9 | Historial cambios remito NO expuesto | BAJO | Auditoría | MEDIO | Postergar |
| R-10 | Remito electrónico NO implementado | BAJO | Fiscal | ALTO | Postergar |

### 4.4 Recomendaciones - REMITOS

#### Acciones Inmediatas (Sprint 1)
1. 🔴 **CRÍTICO: Agregar validación stock** en `EmitirDocumentoVentaCommand` (remitos)
2. ✅ **Crear estructura tipada `RemitoCotAtributos`** con campos específicos
3. ✅ **Agregar validación COT obligatorio** según configuración
4. ✅ **Agregar validación precio valorizado** en `EmitirDocumentoVentaCommand`
5. ✅ **Agregar tests unitarios** para atributos y COT

#### Acciones Corto Plazo (Sprint 2)
1. Completar validaciones cliente y items
2. Implementar servicio de impresión remito
3. Completar tests de workflow completo

---

## 5. MÓDULO: FACTURAS

### 5.1 Backend - Estado Actual

#### 5.1.1 Controllers
| Endpoint | Método | Implementado | Funcionalidad | Legacy Equivalente |
|---|---|---|---|---|
| `/api/ventas/pre-facturas` | POST | ✅ | Crear pre-factura borrador | Crear factura |
| `/api/comprobantes` | GET | ✅ | Listado comprobantes | Listado facturas |
| `/api/comprobantes/{id}` | GET | ✅ | Detalle factura | Ver factura |
| `/api/comprobantes/{id}/emitir` | POST | ⚠️ | Emitir comprobante | Confirmar factura |
| `/api/facturacion/autorizarAfipWsfe` | POST | ✅ | Autorizar AFIP WSFE | Autorización fiscal |
| `/api/facturacion/lote-masivo` | POST | ✅ | Facturar lote masivo | Facturación masiva |
| `/api/ventas/pre-notas-credito` | POST | ✅ | Crear NC borrador | Crear NC |
| `/api/ventas/notas-debito` | POST | ✅ | Crear ND | Crear ND |
| `/api/ventas/notas-debito` | GET | ✅ | Listado notas débito | Listado ND |

#### 5.1.2 Commands Implementados
| Command | Handler | Validaciones | Tests | Notas |
|---|---|---|---|---|
| `CrearBorradorVentaCommand` | ✅ | ⚠️ Parcial | ⚠️ Básico | Crea factura borrador |
| `EmitirComprobanteCommand` | ✅ | ✅ Completo | ⚠️ Parcial | Emite comprobante |
| `AutorizarComprobanteAfipWsfeCommand` | ✅ | ✅ Completo | ❌ | Autoriza AFIP |
| `FacturarDocumentosMasivoCommand` | ✅ | ✅ Completo | ❌ | Factura masivo |
| `RegistrarNotaDebitoVentaCommand` | ✅ | ✅ Completo | ✅ | Crea ND |
| `RegistrarDevolucionVentaCommand` | ✅ | ✅ Completo | ⚠️ | Crea NC devolución |
| `AnularComprobanteCommand` | ✅ | ✅ | ❌ | Anula comprobante |

#### 5.1.3 Queries Implementados
| Query | Handler | DTO Response | Tests | Notas |
|---|---|---|---|---|
| `GetComprobantesPagedQuery` | ✅ | `ComprobanteListDto` | ⚠️ | Lista comprobantes |
| `GetComprobanteDetalleQuery` | ✅ | `ComprobanteDetalleDto` | ⚠️ | Detalle factura |
| `GetNotasDebitoPagedQuery` | ✅ | `ComprobanteListDto` | ❌ | Lista ND |
| `GetNotasDebitoByOrigenQuery` | ✅ | `ComprobanteListDto` | ❌ | ND por comprobante |
| `GetSaldoPendienteTerceroQuery` | ✅ | `SaldoPendienteDto` | ❌ | Saldo cliente |

#### 5.1.4 Validaciones Fiscales Implementadas
| Validación | Implementada | Ubicación | Notas |
|---|---|---|---|
| Tipo comprobante válido | ✅ | `EmitirComprobanteCommand` | OK |
| Cliente con condición IVA compatible | ✅ | `EmitirComprobanteCommand` | OK |
| Punto facturación habilitado | ✅ | `EmitirComprobanteCommand` | OK |
| Timbrado vigente (Paraguay) | ✅ | `EmitirComprobanteCommand` | OK |
| Numeración correlativa | ✅ | `EmitirComprobanteCommand` | OK |
| Ítems con alícuota IVA | ✅ | `EmitirComprobanteCommand` | OK |
| Percepciones aplicadas | ✅ | `ComprobanteTributoResolver` | OK |
| Retenciones aplicadas | ✅ | `ComprobanteTributoResolver` | OK |
| Total comprobante consistente | ✅ | `EmitirComprobanteCommand` | OK |
| CAE AFIP (Argentina) | ✅ | `AutorizarComprobanteAfipWsfeCommand` | OK |

### 5.2 Comparación con Legacy

#### 5.2.1 Funcionalidad Legacy de Facturas
| Funcionalidad | Backend Actual | Gap |
|---|---|---|---|
| Crear factura directa | ✅ | ✅ OK |
| Crear factura desde remito | ✅ | ✅ OK |
| Crear factura desde pedido | ✅ | ✅ OK |
| Emitir factura | ✅ | ✅ OK |
| Autorizar AFIP WSFE | ✅ | ✅ OK |
| Autorizar AFIP WSFEX | ⚠️ | ⚠️ **GAP** |
| Emitir lote masivo | ✅ | ✅ OK |
| Anular factura | ✅ | ✅ OK |
| Crear nota crédito | ✅ | ✅ OK |
| Crear nota débito | ✅ | ✅ OK |
| Aplicar percepciones | ✅ | ✅ OK |
| Aplicar retenciones | ✅ | ✅ OK |
| Ver saldo cliente | ✅ | ✅ OK |
| Imputar cobro | ✅ | ✅ OK |
| Ver cobros aplicados | ✅ | ✅ OK |
| Imprimir factura | ⚠️ | ⚠️ **GAP** |
| Enviar factura email | ❌ | ⚠️ **GAP** |

#### 5.2.2 Validaciones Legacy vs Actual
| Validación | Legacy | Backend Actual | Gap |
|---|---|---|---|
| Cliente activo | ✅ | ⚠️ | ⚠️ No ejecutada |
| Cliente no bloqueado | ✅ | ⚠️ | ⚠️ No ejecutada |
| Cliente con límite crédito | ✅ | ⚠️ | ⚠️ No ejecutada en factura |
| Condición IVA compatible | ✅ | ✅ | ✅ OK |
| Punto facturación habilitado | ✅ | ✅ | ✅ OK |
| Timbrado vigente | ✅ | ✅ | ✅ OK |
| CAE AFIP | ✅ | ✅ | ✅ OK |
| Percepciones correctas | ✅ | ✅ | ✅ OK |
| Total consistente | ✅ | ✅ | ✅ OK |

### 5.3 Gaps Identificados - FACTURAS

#### 5.3.1 Gaps Críticos (Alta Prioridad)
| # | Gap | Impacto | Módulo Afectado | Esfuerzo | Acción Recomendada |
|---|---|---|---|---|---|
| F-1 | Validación cliente bloqueado NO ejecutada | MEDIO | Emitir factura | BAJO | Agregar validación |
| F-2 | Validación límite crédito NO ejecutada | MEDIO | Emitir factura | MEDIO | Agregar validación |

#### 5.3.2 Gaps Medios (Media Prioridad)
| # | Gap | Impacto | Módulo Afectado | Esfuerzo | Acción Recomendada |
|---|---|---|---|---|---|
| F-3 | Autorización AFIP WSFEX NO implementada | MEDIO | Exportación | ALTO | Implementar si necesario |
| F-4 | Impresión factura NO implementada | MEDIO | Imprimir | ALTO | Implementar servicio |
| F-5 | Envío email factura NO implementado | BAJO | Comunicaciones | MEDIO | Postergar |
| F-6 | Tests faltantes para facturación | MEDIO | Testing | ALTO | Agregar tests |

#### 5.3.3 Gaps Bajos (Baja Prioridad)
| # | Gap | Impacto | Módulo Afectado | Esfuerzo | Acción Recomendada |
|---|---|---|---|---|---|
| F-7 | Factura electrónica Paraguay NO completa | BAJO | Fiscal Paraguay | ALTO | Postergar |
| F-8 | Historial cambios factura NO expuesto | BAJO | Auditoría | MEDIO | Postergar |

### 5.4 Recomendaciones - FACTURAS

#### Acciones Inmediatas (Sprint 1)
1. ✅ **Agregar validación cliente bloqueado** en `EmitirComprobanteCommand`
2. ✅ **Agregar validación límite crédito** en `EmitirComprobanteCommand`
3. ✅ **Agregar tests unitarios** para validaciones fiscales

#### Acciones Corto Plazo (Sprint 2)
1. Evaluar necesidad AFIP WSFEX e implementar si aplica
2. Implementar servicio de impresión factura
3. Completar tests de facturación completa

---

## 6. MATRIZ DE PARIDAD CONSOLIDADA

### 6.1 Resumen por Módulo

| Módulo | % Paridad | Gaps Críticos | Gaps Medios | Gaps Bajos | Estado |
|---|---|---|---|---|---|
| **Clientes** | 75% | 6 | 6 | 3 | ⚠️ BUENO con gaps menores |
| **Productos** | 80% | 4 | 9 | 3 | ⚠️ BUENO, falta stock reservado |
| **Pedidos** | 70% | 6 | 7 | 2 | ⚠️ REGULAR, faltan validaciones |
| **Remitos** | 65% | 4 | 4 | 2 | ⚠️ REGULAR, atributos incompletos |
| **Facturas** | 85% | 2 | 4 | 2 | ✅ BUENO, casi completo |
| **GLOBAL** | **75%** | **22** | **30** | **12** | ⚠️ **BUENO CON GAPS** |

### 6.2 Gaps Críticos Consolidados (Top 15)

| Prioridad | # | Gap | Módulos | Esfuerzo | Sprint |
|---|---|---|---|---|---|
| 🔴 1 | P-1 | Stock reservado NO implementado | Productos, Pedidos, Remitos | ALTO | Sprint 1 |
| 🔴 2 | PD-1, R-1 | Validación stock disponible NO ejecutada | Pedidos, Remitos | MEDIO | Sprint 1 |
| 🔴 3 | C-5, PD-2 | Validación límite crédito NO ejecutada | Clientes, Pedidos, Facturas | MEDIO | Sprint 1 |
| 🟠 4 | C-6, PD-3, R-5, F-1 | Validación cliente bloqueado NO ejecutada | Todos | BAJO | Sprint 1 |
| 🟠 5 | C-1 | Domicilio, teléfono, email NO en listado | Clientes | BAJO | Sprint 1 |
| 🟠 6 | C-2 | Vendedor NO visible en selector ventas | Clientes | BAJO | Sprint 1 |
| 🟠 7 | C-3 | Condición pago NO visible en selector | Clientes | BAJO | Sprint 1 |
| 🟠 8 | C-4 | Plazo días NO visible en selector | Clientes | BAJO | Sprint 1 |
| 🟠 9 | P-4 | Validación stock en venta NO ejecutada | Productos | MEDIO | Sprint 1 |
| 🟠 10 | PD-4 | Cambio fecha entrega NO permitido | Pedidos | MEDIO | Sprint 1 |
| 🟠 11 | PD-5 | Depósito origen NO asignado | Pedidos | MEDIO | Sprint 1 |
| 🟠 12 | R-2 | Atributos COT NO tipados | Remitos | MEDIO | Sprint 1 |
| 🟠 13 | R-3 | Validación COT obligatorio NO ejecutada | Remitos | BAJO | Sprint 1 |
| 🟠 14 | PD-12, R-8, F-6 | Tests faltantes | Todos | ALTO | Sprint 1-2 |
| 🟡 15 | P-3 | Moneda descripción NO expuesta | Productos | BAJO | Sprint 2 |

### 6.3 Priorización de Cierre

#### Sprint 1 (2 semanas) - CRÍTICO
**Objetivo:** Cerrar gaps críticos que bloquean operación de ventas

1. **Stock Reservado** (P-1) - 5 días
   - Agregar campo `StockReservado` en `StockItem`
   - Implementar service de reserva
   - Ajustar cálculo stock disponible
   - Tests unitarios

2. **Validaciones de Ventas** (PD-1, R-1, C-5, PD-2, C-6, PD-3, R-5, F-1, P-4) - 3 días
   - Agregar validación stock en commands
   - Agregar validación límite crédito
   - Agregar validación cliente bloqueado
   - Tests unitarios

3. **Ampliar Selector Clientes** (C-1, C-2, C-3, C-4, C-11) - 2 días
   - Ampliar `ClienteSelectorVentasDto`
   - Agregar datos comerciales
   - Tests unitarios

4. **Gestión Pedidos** (PD-4, PD-5) - 2 días
   - Command cambio fecha entrega
   - Agregar campo depósito origen
   - Tests unitarios

5. **Atributos Remito** (R-2, R-3, R-4) - 2 días
   - Crear estructura tipada `RemitoCotAtributos`
   - Agregar validaciones
   - Tests unitarios

#### Sprint 2 (2 semanas) - COMPLETITUD
**Objetivo:** Completar funcionalidad y cerrar gaps medios

1. **Completar Listados** (C-7, C-8, C-9, C-10, P-5 a P-12) - 3 días
   - Agregar filtros faltantes
   - Ampliar DTOs de listado
   - Tests unitarios

2. **Workflow Pedidos** (PD-7, PD-8, PD-9 a PD-11) - 4 días
   - Evaluar modificación pedido emitido
   - Implementar reabrir pedido
   - Completar validaciones
   - Tests de workflow

3. **Servicios de Impresión** (R-7, F-4) - 4 días
   - Implementar servicio impresión remito
   - Implementar servicio impresión factura
   - Tests de impresión

4. **Tests Completos** (PD-13, R-8, F-6) - 3 días
   - Completar tests de queries
   - Completar tests de commands
   - Completar tests de validaciones
   - Tests de integración

---

## 7. PLAN DE IMPLEMENTACIÓN

### 7.1 Secuencia Recomendada

```
┌─────────────────────────────────────────────────────────────────┐
│ SPRINT 1: Cierre Gaps Críticos (2 semanas)                    │
├─────────────────────────────────────────────────────────────────┤
│ Semana 1                                                        │
│ ├─ Días 1-2: Stock Reservado (Domain + Service)                │
│ ├─ Día 3: Validaciones Ventas (Commands)                       │
│ ├─ Día 4: Ampliar Selector Clientes (DTOs)                     │
│ └─ Día 5: Tests Unitarios                                       │
│                                                                  │
│ Semana 2                                                        │
│ ├─ Día 1: Gestión Pedidos (Commands)                           │
│ ├─ Día 2: Atributos Remito (Estructura)                        │
│ ├─ Día 3: Tests Unitarios                                       │
│ ├─ Día 4: Build + Integración                                   │
│ └─ Día 5: Validación + Ajustes                                  │
└─────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────┐
│ SPRINT 2: Completitud Funcional (2 semanas)                   │
├─────────────────────────────────────────────────────────────────┤
│ Semana 1                                                        │
│ ├─ Días 1-2: Completar Listados (Filtros + DTOs)               │
│ ├─ Días 3-4: Workflow Pedidos (Commands)                       │
│ └─ Día 5: Tests Unitarios                                       │
│                                                                  │
│ Semana 2                                                        │
│ ├─ Días 1-2: Servicios Impresión (Remito + Factura)            │
│ ├─ Días 3-4: Tests Completos (Integración)                     │
│ └─ Día 5: Build + Validación Final                              │
└─────────────────────────────────────────────────────────────────┘
```

### 7.2 Recursos Necesarios

| Rol | Sprint 1 | Sprint 2 | Total |
|---|---|---|---|
| Backend Developer Senior | 2 personas | 2 personas | 2 personas |
| Backend Developer Mid | 1 persona | 1 persona | 1 persona |
| QA / Tester | 1 persona (50%) | 1 persona (50%) | 1 persona |
| **Total esfuerzo** | **2.5 FTE** | **2.5 FTE** | **2.5 FTE** |

### 7.3 Riesgos Identificados

| Riesgo | Probabilidad | Impacto | Mitigación |
|---|---|---|---|
| Stock reservado complejo de implementar | MEDIA | ALTO | Diseño detallado previo, spike técnico |
| Validaciones afectan performance | BAJA | MEDIO | Optimizar queries, usar caché |
| Tests descubren bugs existentes | ALTA | MEDIO | Priorizar fixes, agregar regression |
| Frontend desactualizado vs backend | ALTA | ALTO | Coordinación estrecha, versionado API |
| Legacy inconsistente con nuevo | MEDIA | ALTO | Validar contra legacy en paralelo |

### 7.4 Criterios de Éxito

#### Sprint 1
- ✅ Stock reservado funcional y testeado
- ✅ Validaciones críticas implementadas en ventas
- ✅ Selector clientes ampliado y optimizado
- ✅ Gestión básica de pedidos completa
- ✅ Atributos remito tipados y validados
- ✅ Build limpio sin errores
- ✅ 80%+ code coverage en nuevos features

#### Sprint 2
- ✅ Listados completos con todos los filtros
- ✅ Workflow pedidos completo
- ✅ Servicios impresión funcionales
- ✅ Tests completos (unitarios + integración)
- ✅ Build limpio sin errores
- ✅ 85%+ code coverage global
- ✅ Paridad funcional >= 90%

---

## 8. CONCLUSIONES

### 8.1 Estado Actual
El backend de ventas de **ZuluIA_Back** tiene una **base sólida** con aproximadamente **75% de paridad funcional** respecto al legado `C:\Zulu\zuluApp`. Los módulos de **Facturas** y **Productos** están muy bien cubiertos (80-85%), mientras que **Pedidos** y **Remitos** necesitan más trabajo (65-70%).

### 8.2 Gaps Principales
Los gaps críticos se concentran en:
1. **Stock reservado** (no implementado)
2. **Validaciones de negocio** (no ejecutadas en ventas)
3. **Información comercial** (no expuesta en selectores)
4. **Workflow de pedidos** (incompleto)
5. **Tests unitarios** (cobertura insuficiente)

### 8.3 Viabilidad
Es **100% viable** cerrar la paridad funcional al **90%+** en **4 semanas** (2 sprints) con el equipo y enfoque correctos. Los cambios son mayormente **incrementales** y **no requieren refactoring arquitectónico**.

### 8.4 Impacto en Frontend
El frontend **ZuluIA_Front** podrá consumir el backend completo una vez cerrados los gaps críticos del **Sprint 1**. El **Sprint 2** agregará **confort y completitud**, pero no bloqueará funcionalidad básica.

### 8.5 Recomendación Final
**Priorizar Sprint 1 de inmediato** para desbloquear el frontend y permitir desarrollo paralelo. El **Sprint 2** puede ejecutarse en paralelo con el desarrollo del frontend sin bloqueos.

---

## APÉNDICES

### A.1 Referencias Legacy
- `C:\Zulu\zuluApp\ASP\CLIENTES_Listado.asp`
- `C:\Zulu\zuluApp\ASP\ConsultarClientes_Listado.asp`
- `C:\Zulu\zuluApp\ASP\FichaDeCliente.asp`
- `C:\Zulu\zuluApp\ASP\ITEMSNOFINANCIEROS_Listado.asp`
- `C:\Zulu\zuluApp\ASP\ITEMSNUEVOSTOCK_Listado.asp`
- `C:\Zulu\zuluApp\ASP\ITEMS_EditarForm.asp`

### A.2 Archivos Backend Clave
- `src/ZuluIA_Back.Api/Controllers/VentasController.cs`
- `src/ZuluIA_Back.Api/Controllers/TercerosController.cs`
- `src/ZuluIA_Back.Api/Controllers/ItemsController.cs`
- `src/ZuluIA_Back.Api/Controllers/ComprobantesController.cs`
- `src/ZuluIA_Back.Application/Features/Ventas/*`
- `src/ZuluIA_Back.Application/Features/Terceros/*`
- `src/ZuluIA_Back.Application/Features/Items/*`
- `src/ZuluIA_Back.Application/Features/Comprobantes/*`

### A.3 Tests Existentes
- `tests/ZuluIA_Back.UnitTests/Application/GetClientesSelectorVentasQueryHandlerTests.cs`
- `tests/ZuluIA_Back.UnitTests/Application/GetTerceroByNroDocumentoQueryHandlerTests.cs`
- `tests/ZuluIA_Back.UnitTests/Api/TercerosControllerTests.cs`
- `tests/ZuluIA_Back.UnitTests/Api/VentasControllerNotasDebitoTests.cs`
- `tests/ZuluIA_Back.UnitTests/Application/GetItemsPagedQueryHandlerTests.cs`
- `tests/ZuluIA_Back.UnitTests/Application/GetItemsVendiblesQueryHandlerTests.cs`

---

**Fin del Documento**  
**Versión:** 1.0  
**Última actualización:** 2025-01-20
