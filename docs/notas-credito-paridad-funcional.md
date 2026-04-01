# Paridad Funcional: Notas de Crédito de Ventas

## Objetivo
Lograr paridad funcional 100% entre `C:\Zulu\ZuluIA_Front\app\ventas\notas-credito` y los formularios legacy de `C:\Zulu\zuluApp` relacionados con notas de crédito de ventas.

## Referencias Legacy (zuluApp)

### Formularios identificados
- `frmNotaCreditoVenta` - Formulario principal de NC
- `frmPreNotaCreditoVenta` - Pre-NC (borrador)
- `frmNotaCreditoVentaForExport` - Exportación
- `frmNotaCreditoVentaImport` - Importación
- Relacionados: `frmFacturaVenta`, `frmImputacionesVentas`, `frmDesimputarComprobantesVentas`

## 1. CAMPOS DE CABECERA DE NOTA DE CRÉDITO

### 1.1 Identificación Básica
| Campo | zuluApp | Backend Actual | Estado | Acción Requerida |
|-------|---------|----------------|--------|------------------|
| ID interno | ✅ | ✅ | COMPLETO | - |
| Sucursal | ✅ | ✅ | COMPLETO | - |
| Punto de facturación | ✅ | ✅ | COMPLETO | - |
| Tipo comprobante | ✅ | ✅ | COMPLETO | - |
| Prefijo | ✅ | ✅ | COMPLETO | - |
| Número | ✅ | ✅ | COMPLETO | - |
| Número formateado | ✅ | ✅ | COMPLETO | - |
| Fecha | ✅ | ✅ | COMPLETO | - |
| Fecha vencimiento | ✅ | ✅ | COMPLETO | - |

### 1.2 Tercero (Cliente)
| Campo | zuluApp | Backend Actual | Estado | Acción Requerida |
|-------|---------|----------------|--------|------------------|
| TerceroId | ✅ | ✅ | COMPLETO | - |
| Razón social | ✅ | ✅ | COMPLETO | - |
| CUIT/RUC | ✅ | ✅ | COMPLETO | - |
| Condición IVA | ✅ | ✅ | COMPLETO | - |
| Domicilio snapshot | ✅ | ✅ | COMPLETO | - |
| Legajo cliente | ✅ | ❌ | FALTANTE | Agregar TerceroLegajo al DTO |

### 1.3 Datos Comerciales
| Campo | zuluApp | Backend Actual | Estado | Acción Requerida |
|-------|---------|----------------|--------|------------------|
| Vendedor ID | ✅ | ✅ | COMPLETO | - |
| Vendedor nombre | ✅ | ✅ | COMPLETO | - |
| Vendedor legajo | ✅ | ✅ | COMPLETO | - |
| Cobrador ID | ✅ | ✅ | COMPLETO | - |
| Cobrador nombre | ✅ | ✅ | COMPLETO | - |
| Cobrador legajo | ✅ | ✅ | COMPLETO | - |
| Zona comercial ID | ✅ | ✅ | COMPLETO | - |
| Zona comercial descripción | ✅ | ✅ | COMPLETO | - |
| Lista de precios ID | ✅ | ✅ | COMPLETO | - |
| Lista de precios descripción | ✅ | ✅ | COMPLETO | - |
| Condición de pago ID | ✅ | ✅ | COMPLETO | - |
| Condición de pago descripción | ✅ | ✅ | COMPLETO | - |
| Plazo días | ✅ | ✅ | COMPLETO | - |
| Canal de venta ID | ✅ | ✅ | COMPLETO | - |
| Canal de venta descripción | ✅ | ✅ | COMPLETO | - |
| % comisión vendedor | ✅ | ✅ | COMPLETO | - |
| Importe comisión vendedor | ✅ | ✅ | COMPLETO | - |
| % comisión cobrador | ✅ | ✅ | COMPLETO | - |
| Importe comisión cobrador | ✅ | ✅ | COMPLETO | - |

### 1.4 Datos Logísticos
| Campo | zuluApp | Backend Actual | Estado | Acción Requerida |
|-------|---------|----------------|--------|------------------|
| Transporte ID | ✅ | ✅ | COMPLETO | - |
| Transporte razón social | ✅ | ✅ | COMPLETO | - |
| Chofer nombre | ✅ | ✅ | COMPLETO | - |
| Chofer DNI | ✅ | ✅ | COMPLETO | - |
| Patente vehículo | ✅ | ✅ | COMPLETO | - |
| Patente acoplado | ✅ | ✅ | COMPLETO | - |
| Domicilio entrega | ✅ | ✅ | COMPLETO | - |
| Observaciones logísticas | ✅ | ✅ | COMPLETO | - |
| Fecha estimada entrega | ✅ | ✅ | COMPLETO | - |
| Fecha real entrega | ✅ | ✅ | COMPLETO | - |
| Firma conformidad | ✅ | ✅ | COMPLETO | - |
| Nombre quien recibe | ✅ | ✅ | COMPLETO | - |

### 1.5 Moneda y Cotización
| Campo | zuluApp | Backend Actual | Estado | Acción Requerida |
|-------|---------|----------------|--------|------------------|
| Moneda ID | ✅ | ✅ | COMPLETO | - |
| Moneda símbolo | ✅ | ✅ | COMPLETO | - |
| Cotización | ✅ | ✅ | COMPLETO | - |

### 1.6 Observaciones
| Campo | zuluApp | Backend Actual | Estado | Acción Requerida |
|-------|---------|----------------|--------|------------------|
| Observación (pública) | ✅ | ✅ | COMPLETO | - |
| Observación interna | ✅ | ✅ | COMPLETO | - |
| Observación fiscal | ✅ | ✅ | COMPLETO | - |

### 1.7 Totales y Cálculos
| Campo | zuluApp | Backend Actual | Estado | Acción Requerida |
|-------|---------|----------------|--------|------------------|
| Subtotal | ✅ | ✅ | COMPLETO | - |
| Descuento % | ✅ | ✅ | COMPLETO | - |
| Descuento importe | ✅ | ✅ | COMPLETO | - |
| Recargo % | ✅ | ✅ | COMPLETO | - |
| Recargo importe | ✅ | ✅ | COMPLETO | - |
| Neto gravado | ✅ | ✅ | COMPLETO | - |
| Neto no gravado | ✅ | ✅ | COMPLETO | - |
| IVA RI | ✅ | ✅ | COMPLETO | - |
| IVA RNI | ✅ | ✅ | COMPLETO | - |
| Percepciones | ✅ | ✅ | COMPLETO | - |
| Retenciones | ✅ | ✅ | COMPLETO | - |
| Total | ✅ | ✅ | COMPLETO | - |
| Saldo | ✅ | ✅ | COMPLETO | - |

### 1.8 Datos Fiscales AFIP
| Campo | zuluApp | Backend Actual | Estado | Acción Requerida |
|-------|---------|----------------|--------|------------------|
| CAE | ✅ | ✅ | COMPLETO | - |
| CAEA | ✅ | ✅ | COMPLETO | - |
| Fecha vto CAE | ✅ | ✅ | COMPLETO | - |
| QR Data | ✅ | ✅ | COMPLETO | - |
| Estado AFIP | ✅ | ✅ | COMPLETO | - |
| Último error AFIP | ✅ | ✅ | COMPLETO | - |
| Fecha última consulta AFIP | ✅ | ✅ | COMPLETO | - |

### 1.9 Datos Fiscales SIFEN (Paraguay)
| Campo | zuluApp | Backend Actual | Estado | Acción Requerida |
|-------|---------|----------------|--------|------------------|
| Timbrado ID | ✅ | ✅ | COMPLETO | - |
| Nro Timbrado | ✅ | ✅ | COMPLETO | - |
| Estado SIFEN | ✅ | ✅ | COMPLETO | - |
| Código respuesta | ✅ | ✅ | COMPLETO | - |
| Mensaje respuesta | ✅ | ✅ | COMPLETO | - |
| Tracking ID | ✅ | ✅ | COMPLETO | - |
| CDC | ✅ | ✅ | COMPLETO | - |
| Número de lote | ✅ | ✅ | COMPLETO | - |
| Fecha respuesta | ✅ | ✅ | COMPLETO | - |
| Tiene identificadores SIFEN | ✅ | ✅ | COMPLETO | - |
| Puede reintentar SIFEN | ✅ | ✅ | COMPLETO | - |
| Puede conciliar SIFEN | ✅ | ✅ | COMPLETO | - |

### 1.10 Comprobante Origen
| Campo | zuluApp | Backend Actual | Estado | Acción Requerida |
|-------|---------|----------------|--------|------------------|
| Comprobante origen ID | ✅ | ✅ | COMPLETO | - |
| Comprobante origen número | ✅ | ✅ | COMPLETO | - |
| Comprobante origen tipo | ✅ | ✅ | COMPLETO | - |

### 1.11 Estado y Auditoría
| Campo | zuluApp | Backend Actual | Estado | Acción Requerida |
|-------|---------|----------------|--------|------------------|
| Estado | ✅ | ✅ | COMPLETO | - |
| Fecha creación | ✅ | ✅ | COMPLETO | - |
| Fecha actualización | ✅ | ✅ | COMPLETO | - |
| Usuario creador | ✅ | ❌ | FALTANTE | Agregar UsuarioCreadorId y Nombre |
| Usuario modificador | ✅ | ❌ | FALTANTE | Agregar UsuarioModificadorId y Nombre |

## 2. CAMPOS DE DETALLE (ITEMS)

### 2.1 Identificación del Item
| Campo | zuluApp | Backend Actual | Estado | Acción Requerida |
|-------|---------|----------------|--------|------------------|
| Item ID | ✅ | ✅ | COMPLETO | - |
| Código item | ✅ | ✅ | COMPLETO | - |
| Descripción | ✅ | ✅ | COMPLETO | - |
| Descripción extendida | ✅ | ✅ | COMPLETO | - |

### 2.2 Cantidades y Precios
| Campo | zuluApp | Backend Actual | Estado | Acción Requerida |
|-------|---------|----------------|--------|------------------|
| Cantidad | ✅ | ✅ | COMPLETO | - |
| Precio unitario | ✅ | ✅ | COMPLETO | - |
| Descuento % | ✅ | ✅ | COMPLETO | - |
| Descuento importe | ✅ | ✅ | COMPLETO | - |
| IVA % | ✅ | ✅ | COMPLETO | - |
| IVA importe | ✅ | ✅ | COMPLETO | - |
| Subtotal | ✅ | ✅ | COMPLETO | - |
| Total | ✅ | ✅ | COMPLETO | - |
| Precio lista original | ✅ | ❌ | FALTANTE | Agregar PrecioListaOriginal |

### 2.3 Stock y Logística
| Campo | zuluApp | Backend Actual | Estado | Acción Requerida |
|-------|---------|----------------|--------|------------------|
| Depósito ID | ✅ | ✅ | COMPLETO | - |
| Depósito descripción | ✅ | ✅ | COMPLETO | - |
| Lote | ✅ | ❌ | FALTANTE | Agregar Lote |
| Serie | ✅ | ❌ | FALTANTE | Agregar Serie |
| Vencimiento | ✅ | ❌ | FALTANTE | Agregar FechaVencimiento |
| Unidad de medida | ✅ | ❌ | FALTANTE | Agregar UnidadMedidaId y Descripción |

### 2.4 Atributos Comerciales
| Campo | zuluApp | Backend Actual | Estado | Acción Requerida |
|-------|---------|----------------|--------|------------------|
| Atributos comerciales | ✅ | ✅ | COMPLETO | - |
| Observación por renglón | ✅ | ❌ | FALTANTE | Agregar ObservacionRenglon |
| Comisión vendedor renglón | ✅ | ❌ | FALTANTE | Agregar ComisionVendedorRenglon si aplica |

### 2.5 Trazabilidad
| Campo | zuluApp | Backend Actual | Estado | Acción Requerida |
|-------|---------|----------------|--------|------------------|
| Renglón origen ID | ✅ | ❌ | FALTANTE | Agregar ComprobanteItemOrigenId |
| Cantidad pendiente | ✅ | ❌ | FALTANTE | Agregar CantidadPendiente si aplica |

## 3. OPERACIONES Y FLUJOS

### 3.1 Creación de Nota de Crédito
| Operación | zuluApp | Backend Actual | Estado | Acción Requerida |
|-----------|---------|----------------|--------|------------------|
| Crear NC desde factura | ✅ | ✅ | COMPLETO | Validar flujo completo |
| Crear NC manual | ✅ | ✅ | COMPLETO | Validar flujo completo |
| Crear pre-NC (borrador) | ✅ | ✅ | COMPLETO | Endpoint /pre-notas-credito existe |
| Validar comprobante origen | ✅ | ⚠️ | PARCIAL | Fortalecer validaciones |
| Validar montos no excedan original | ✅ | ⚠️ | PARCIAL | Implementar validación estricta |
| Copiar datos comerciales origen | ✅ | ⚠️ | PARCIAL | Verificar copia completa |

### 3.2 Emisión y Fiscalización
| Operación | zuluApp | Backend Actual | Estado | Acción Requerida |
|-----------|---------|----------------|--------|------------------|
| Emitir NC | ✅ | ✅ | COMPLETO | - |
| Obtener CAE | ✅ | ✅ | COMPLETO | - |
| Obtener CDC (SIFEN) | ✅ | ✅ | COMPLETO | - |
| Reintentar fiscalización | ✅ | ✅ | COMPLETO | - |
| Consultar estado fiscal | ✅ | ✅ | COMPLETO | - |

### 3.3 Impactos Operativos
| Operación | zuluApp | Backend Actual | Estado | Acción Requerida |
|-----------|---------|----------------|--------|------------------|
| Acreditar cuenta corriente | ✅ | ✅ | COMPLETO | - |
| Reingresar stock | ✅ | ✅ | COMPLETO | - |
| Ajustar comisiones | ✅ | ❌ | FALTANTE | Implementar ajuste de comisiones |
| Registrar en libro IVA | ✅ | ✅ | COMPLETO | - |

### 3.4 Imputación y Vinculación
| Operación | zuluApp | Backend Actual | Estado | Acción Requerida |
|-----------|---------|----------------|--------|------------------|
| Imputar NC contra factura | ✅ | ✅ | COMPLETO | - |
| Desimputar NC | ✅ | ✅ | COMPLETO | - |
| Vincular NC a factura | ✅ | ✅ | COMPLETO | - |
| Ver imputaciones de NC | ✅ | ✅ | COMPLETO | - |

### 3.5 Anulación y Modificación
| Operación | zuluApp | Backend Actual | Estado | Acción Requerida |
|-----------|---------|----------------|--------|------------------|
| Anular NC | ✅ | ✅ | COMPLETO | - |
| Modificar NC borrador | ✅ | ✅ | COMPLETO | - |
| Bloquear NC emitida | ✅ | ✅ | COMPLETO | - |
| Bloquear NC fiscalizada | ✅ | ✅ | COMPLETO | - |

## 4. LISTADOS Y FILTROS

### 4.1 Filtros de Búsqueda
| Filtro | zuluApp | Backend Necesario | Estado | Acción Requerida |
|--------|---------|-------------------|--------|------------------|
| Por número | ✅ | ✅ | COMPLETO | - |
| Por fecha desde/hasta | ✅ | ✅ | COMPLETO | - |
| Por cliente | ✅ | ✅ | COMPLETO | - |
| Por estado | ✅ | ✅ | COMPLETO | - |
| Por vendedor | ✅ | ❌ | FALTANTE | Agregar filtro por VendedorId |
| Por cobrador | ✅ | ❌ | FALTANTE | Agregar filtro por CobradorId |
| Por zona comercial | ✅ | ❌ | FALTANTE | Agregar filtro por ZonaComercialId |
| Por punto facturación | ✅ | ✅ | COMPLETO | - |
| Por sucursal | ✅ | ✅ | COMPLETO | - |
| Por tipo comprobante | ✅ | ✅ | COMPLETO | - |
| Por estado fiscal | ✅ | ✅ | COMPLETO | - |
| NC con saldo | ✅ | ❌ | FALTANTE | Agregar filtro ConSaldo |
| NC imputadas | ✅ | ❌ | FALTANTE | Agregar filtro Imputadas |

### 4.2 Ordenamiento
| Orden | zuluApp | Backend Necesario | Estado | Acción Requerida |
|-------|---------|-------------------|--------|------------------|
| Por fecha | ✅ | ✅ | COMPLETO | - |
| Por número | ✅ | ✅ | COMPLETO | - |
| Por cliente | ✅ | ✅ | COMPLETO | - |
| Por total | ✅ | ❌ | FALTANTE | Agregar ordenamiento por Total |
| Por saldo | ✅ | ❌ | FALTANTE | Agregar ordenamiento por Saldo |

## 5. REPORTES Y EXPORTACIONES

### 5.1 Reportes
| Reporte | zuluApp | Backend Necesario | Estado | Acción Requerida |
|---------|---------|-------------------|--------|------------------|
| Listado de NC | ✅ | ✅ | COMPLETO | - |
| NC por cliente | ✅ | ⚠️ | PARCIAL | Endpoint dedicado recomendado |
| NC por período | ✅ | ⚠️ | PARCIAL | Endpoint dedicado recomendado |
| NC con saldo | ✅ | ❌ | FALTANTE | Crear endpoint específico |
| Análisis de devoluciones | ✅ | ❌ | FALTANTE | Crear endpoint estadístico |

### 5.2 Exportaciones
| Exportación | zuluApp | Backend Necesario | Estado | Acción Requerida |
|-------------|---------|-------------------|--------|------------------|
| Excel | ✅ | ⚠️ | PARCIAL | Endpoint genérico de reportes |
| PDF | ✅ | ⚠️ | PARCIAL | Implementar generación PDF |
| Formato contable | ✅ | ❌ | FALTANTE | Definir formato y crear endpoint |

## 6. VALIDACIONES ESPECÍFICAS DE NC

### 6.1 Validaciones de Negocio
| Validación | zuluApp | Backend Actual | Estado | Acción Requerida |
|------------|---------|----------------|--------|------------------|
| Comprobante origen existe | ✅ | ✅ | COMPLETO | - |
| Comprobante origen es factura válida | ✅ | ⚠️ | PARCIAL | Fortalecer validación |
| Estado comprobante origen permite NC | ✅ | ⚠️ | PARCIAL | Validar estado específico |
| Montos NC no exceden factura | ✅ | ❌ | FALTANTE | Implementar validación estricta |
| Items corresponden a factura | ✅ | ❌ | FALTANTE | Validar items vs factura |
| Cantidades no exceden facturadas | ✅ | ❌ | FALTANTE | Validar cantidades por item |
| Cliente coincide con factura | ✅ | ✅ | COMPLETO | - |
| Moneda coincide con factura | ✅ | ✅ | COMPLETO | - |

### 6.2 Validaciones Fiscales
| Validación | zuluApp | Backend Actual | Estado | Acción Requerida |
|------------|---------|----------------|--------|------------------|
| Tipo comprobante fiscal válido | ✅ | ✅ | COMPLETO | - |
| Letra fiscal coincide con cliente | ✅ | ✅ | COMPLETO | - |
| Punto facturación activo | ✅ | ✅ | COMPLETO | - |
| Timbrado vigente (Paraguay) | ✅ | ✅ | COMPLETO | - |
| Numeración disponible | ✅ | ✅ | COMPLETO | - |

## 7. RESUMEN DE GAPS CRÍTICOS

### 7.1 Alta Prioridad (Bloqueantes)
1. **Validación de montos**: Implementar validación estricta que NC no exceda factura
2. **Validación de items**: Verificar que items y cantidades correspondan a factura origen
3. **Filtros comerciales**: Agregar filtros por vendedor, cobrador, zona comercial
4. **Ajuste de comisiones**: Implementar lógica para ajustar comisiones en NC

### 7.2 Media Prioridad (Funcionalidad completa)
1. **Campos de item**: Agregar lote, serie, vencimiento, unidad medida
2. **Observación por renglón**: Permitir observaciones específicas por item
3. **Auditoría**: Agregar usuario creador y modificador
4. **Reportes específicos**: Crear endpoints dedicados para reportes de NC
5. **Ordenamiento extendido**: Agregar ordenamiento por total y saldo

### 7.3 Baja Prioridad (Mejoras)
1. **Precio lista original**: Mostrar precio original antes de descuentos
2. **Trazabilidad de renglón**: Vincular renglones de NC con renglones de factura
3. **Exportaciones avanzadas**: PDF, formato contable
4. **Análisis estadístico**: Dashboard de devoluciones y tendencias

## 8. PLAN DE IMPLEMENTACIÓN

### Fase 1: Validaciones Críticas (Sprint actual)
- [ ] Implementar validación de montos totales NC vs factura
- [ ] Implementar validación de items y cantidades
- [ ] Agregar validación de estado de factura origen
- [ ] Crear tests unitarios para validaciones

### Fase 2: Campos Faltantes (Sprint actual)
- [ ] Agregar campos de auditoría (usuario creador/modificador)
- [ ] Agregar campos de item (lote, serie, vencimiento)
- [ ] Agregar filtros comerciales en queries
- [ ] Migración de base de datos

### Fase 3: Funcionalidad Extendida (Siguiente sprint)
- [ ] Implementar ajuste automático de comisiones
- [ ] Crear endpoints de reportes específicos
- [ ] Implementar generación de PDF
- [ ] Agregar observación por renglón

### Fase 4: Mejoras y Optimización (Backlog)
- [ ] Dashboard de análisis de devoluciones
- [ ] Exportaciones en múltiples formatos
- [ ] Trazabilidad detallada de renglones
- [ ] Auditoría extendida de cambios

## 9. CRITERIOS DE ACEPTACIÓN

Una Nota de Crédito se considera con paridad funcional 100% cuando:

✅ **Datos completos**: Todos los campos visibles en zuluApp están disponibles en el backend
✅ **Validaciones**: Todas las reglas de negocio del legacy están implementadas
✅ **Flujos operativos**: Creación, emisión, fiscalización, imputación funcionan igual que legacy
✅ **Filtros y búsquedas**: Todos los filtros del legacy están disponibles
✅ **Reportes básicos**: Listados y consultas principales funcionan
✅ **Trazabilidad**: Se puede rastrear origen, impactos y modificaciones
✅ **Integración**: NC se integra correctamente con cuenta corriente, stock, comisiones
✅ **Fiscalización**: AFIP y SIFEN funcionan correctamente
✅ **Tests**: Cobertura de tests unitarios >= 80%
✅ **Documentación**: API documentada y casos de uso claros

## 10. REFERENCIAS

- Documento de gaps general: `docs/auditoria-ventas-facturacion-gaps.md`
- Matriz funcional ventas: `docs/matriz-funcional-ventas-zuluapp.md`
- Backend ventas backlog: `docs/ventas-clientes-backlog-backend.md`
- Legacy zuluApp: `C:\Zulu\zuluApp`
- Frontend nuevo: `C:\Zulu\ZuluIA_Front\app\ventas\notas-credito`

---

**Última actualización**: 2024-03-20
**Responsable**: Equipo Backend ZuluIA
**Estado**: En progreso - Fase 1
