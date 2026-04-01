# Paridad Funcional: Notas de Débito de Ventas

## Objetivo
Lograr paridad funcional 100% entre `C:\Zulu\ZuluIA_Front\app\ventas\notas-debito` y los formularios legacy de `C:\Zulu\zuluApp` relacionados con notas de débito de ventas.

## Referencias Legacy (zuluApp)

### Formularios identificados
- `frmNotaDebitoVenta` - Formulario principal de ND
- `frmNotaDebitoVenta2` - Versión alternativa/extendida de ND
- `frmNotaDebitoVentaForExport` - Exportación
- `frmNotaDebitoVentaImport` - Importación
- Relacionados: `frmFacturaVenta`, `frmImputacionesVentas`, `frmVincularComprobantesVenta`

## 1. CAMPOS DE CABECERA DE NOTA DE DÉBITO

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

### 1.4 Moneda y Cotización
| Campo | zuluApp | Backend Actual | Estado | Acción Requerida |
|-------|---------|----------------|--------|------------------|
| Moneda ID | ✅ | ✅ | COMPLETO | - |
| Moneda símbolo | ✅ | ✅ | COMPLETO | - |
| Cotización | ✅ | ✅ | COMPLETO | - |

### 1.5 Observaciones
| Campo | zuluApp | Backend Actual | Estado | Acción Requerida |
|-------|---------|----------------|--------|------------------|
| Observación (pública) | ✅ | ✅ | COMPLETO | - |
| Observación interna | ✅ | ✅ | COMPLETO | - |
| Observación fiscal | ✅ | ✅ | COMPLETO | - |

### 1.6 Totales y Cálculos
| Campo | zuluApp | Backend Actual | Estado | Acción Requerida |
|-------|---------|----------------|--------|------------------|
| Subtotal | ✅ | ✅ | COMPLETO | - |
| Descuento % | ✅ | ✅ | COMPLETO | - |
| Descuento importe | ✅ | ✅ | COMPLETO | - |
| Recargo % | ✅ | ✅ | COMPLETO | - |
| Recargo importe | ✅ | ✅ | COMPLETO | - |
| Neto gravado | ✅ | ✅ | COMPLETO | - |
| Neto no gravado | ✅ | ✅ | COMPLETO | - |
| IVA (RI) | ✅ | ✅ | COMPLETO | - |
| IVA (RNI) | ✅ | ✅ | COMPLETO | - |
| Percepciones | ✅ | ✅ | COMPLETO | - |
| Retenciones | ✅ | ✅ | COMPLETO | - |
| Total | ✅ | ✅ | COMPLETO | - |
| Saldo | ✅ | ✅ | COMPLETO | - |

### 1.7 Datos Fiscales (AFIP / SIFEN)
| Campo | zuluApp | Backend Actual | Estado | Acción Requerida |
|-------|---------|----------------|--------|------------------|
| Timbrado ID | ✅ | ✅ | COMPLETO | - |
| Nro Timbrado | ✅ | ✅ | COMPLETO | - |
| CAE | ✅ | ✅ | COMPLETO | - |
| CAEA | ✅ | ✅ | COMPLETO | - |
| Fecha vencimiento CAE | ✅ | ✅ | COMPLETO | - |
| QR Data | ✅ | ✅ | COMPLETO | - |
| Estado AFIP | ✅ | ✅ | COMPLETO | - |
| Estado SIFEN | ✅ | ✅ | COMPLETO | - |
| SIFEN CDC | ✅ | ✅ | COMPLETO | - |
| SIFEN Tracking ID | ✅ | ✅ | COMPLETO | - |
| SIFEN Código respuesta | ✅ | ✅ | COMPLETO | - |
| SIFEN Mensaje respuesta | ✅ | ✅ | COMPLETO | - |
| Último error AFIP | ✅ | ✅ | COMPLETO | - |

### 1.8 Referencia a Documento Origen
| Campo | zuluApp | Backend Actual | Estado | Acción Requerida |
|-------|---------|----------------|--------|------------------|
| Comprobante origen ID | ✅ | ✅ | COMPLETO | - |
| Tipo comprobante origen | ✅ | ❌ | FALTANTE | Agregar TipoComprobanteOrigenDescripcion |
| Número comprobante origen | ✅ | ❌ | FALTANTE | Agregar ComprobanteOrigenNumero |
| Fecha comprobante origen | ✅ | ❌ | FALTANTE | Agregar ComprobanteOrigenFecha |

### 1.9 Motivo de la Nota de Débito
| Campo | zuluApp | Backend Actual | Estado | Acción Requerida |
|-------|---------|----------------|--------|------------------|
| Motivo ID | ✅ | ❌ | FALTANTE | Agregar MotivoDebitoId |
| Motivo descripción | ✅ | ❌ | FALTANTE | Agregar MotivoDebitoDescripcion |
| Motivo observación | ✅ | ❌ | FALTANTE | Agregar MotivoDebitoObservacion |
| Es fiscal | ✅ | ❌ | FALTANTE | Agregar MotivoDebitoEsFiscal |

### 1.10 Auditoría y Trazabilidad
| Campo | zuluApp | Backend Actual | Estado | Acción Requerida |
|-------|---------|----------------|--------|------------------|
| Estado comprobante | ✅ | ✅ | COMPLETO | - |
| Fecha creación | ✅ | ✅ | COMPLETO | - |
| Usuario creación | ✅ | ✅ | COMPLETO | - |
| Fecha última modificación | ✅ | ✅ | COMPLETO | - |
| Usuario última modificación | ✅ | ✅ | COMPLETO | - |
| Fecha anulación | ✅ | ❌ | FALTANTE | Agregar FechaAnulacion |
| Usuario anulación | ✅ | ❌ | FALTANTE | Agregar UsuarioAnulacionId |
| Motivo anulación | ✅ | ❌ | FALTANTE | Agregar MotivoAnulacion |

## 2. DETALLE DE RENGLONES (ComprobanteItem)

### 2.1 Identificación del Item
| Campo | zuluApp | Backend Actual | Estado | Acción Requerida |
|-------|---------|----------------|--------|------------------|
| Item ID | ✅ | ✅ | COMPLETO | - |
| Item código | ✅ | ✅ | COMPLETO | - |
| Descripción | ✅ | ✅ | COMPLETO | - |
| Orden | ✅ | ✅ | COMPLETO | - |

### 2.2 Cantidades y Precios
| Campo | zuluApp | Backend Actual | Estado | Acción Requerida |
|-------|---------|----------------|--------|------------------|
| Cantidad | ✅ | ✅ | COMPLETO | - |
| Cantidad bonificada | ✅ | ✅ | COMPLETO | - |
| Precio unitario | ✅ | ✅ | COMPLETO | - |
| Precio lista original | ✅ | ✅ | COMPLETO | - |
| Descuento % | ✅ | ✅ | COMPLETO | - |

### 2.3 Impuestos
| Campo | zuluApp | Backend Actual | Estado | Acción Requerida |
|-------|---------|----------------|--------|------------------|
| Alícuota IVA ID | ✅ | ✅ | COMPLETO | - |
| Porcentaje IVA | ✅ | ✅ | COMPLETO | - |
| Es gravado | ✅ | ✅ | COMPLETO | - |

### 2.4 Totales por Renglón
| Campo | zuluApp | Backend Actual | Estado | Acción Requerida |
|-------|---------|----------------|--------|------------------|
| Subtotal neto | ✅ | ✅ | COMPLETO | - |
| IVA importe | ✅ | ✅ | COMPLETO | - |
| Total línea | ✅ | ✅ | COMPLETO | - |

### 2.5 Datos Logísticos del Renglón
| Campo | zuluApp | Backend Actual | Estado | Acción Requerida |
|-------|---------|----------------|--------|------------------|
| Depósito ID | ✅ | ✅ | COMPLETO | - |
| Depósito descripción | ✅ | ✅ | COMPLETO | - |

### 2.6 Datos Extendidos del Renglón
| Campo | zuluApp | Backend Actual | Estado | Acción Requerida |
|-------|---------|----------------|--------|------------------|
| Lote | ✅ | ✅ | COMPLETO | - |
| Serie | ✅ | ✅ | COMPLETO | - |
| Fecha vencimiento | ✅ | ✅ | COMPLETO | - |
| Unidad de medida ID | ✅ | ✅ | COMPLETO | - |
| Unidad de medida descripción | ✅ | ✅ | COMPLETO | - |
| Observación renglón | ✅ | ✅ | COMPLETO | - |
| Comisión vendedor renglón | ✅ | ✅ | COMPLETO | - |
| Comprobante item origen ID | ✅ | ✅ | COMPLETO | - |

### 2.7 Atributos Comerciales del Renglón
| Campo | zuluApp | Backend Actual | Estado | Acción Requerida |
|-------|---------|----------------|--------|------------------|
| Atributos | ✅ | ✅ | COMPLETO | - |

### 2.8 Referencia a Documento Origen por Renglón
| Campo | zuluApp | Backend Actual | Estado | Acción Requerida |
|-------|---------|----------------|--------|------------------|
| Cantidad origen | ✅ | ❌ | FALTANTE | Agregar CantidadDocumentoOrigen |
| Precio origen | ✅ | ❌ | FALTANTE | Agregar PrecioDocumentoOrigen |
| Diferencia cantidad | ✅ | ❌ | FALTANTE | Calcular (opcional) |
| Diferencia precio | ✅ | ❌ | FALTANTE | Calcular (opcional) |

## 3. CATÁLOGOS ESPECÍFICOS DE NOTAS DE DÉBITO

### 3.1 Motivos de Débito
| Campo | Backend Actual | Estado | Acción Requerida |
|-------|----------------|--------|------------------|
| MotivoDebito (entidad) | ❌ | FALTANTE | Crear entidad MotivoDebito |
| Código | ❌ | FALTANTE | Agregar campo Codigo |
| Descripción | ❌ | FALTANTE | Agregar campo Descripcion |
| Es fiscal | ❌ | FALTANTE | Agregar campo EsFiscal |
| Requiere documento origen | ❌ | FALTANTE | Agregar campo RequiereDocumentoOrigen |
| Afecta cuenta corriente | ❌ | FALTANTE | Agregar campo AfectaCuentaCorriente |
| Activo | ❌ | FALTANTE | Agregar campo Activo |

**Motivos comunes en zuluApp:**
- Diferencia de precio
- Faltante de mercadería
- Error en facturación
- Gastos adicionales no incluidos
- Intereses por mora
- Ajuste de redondeo
- Fletes no facturados
- Recargo por forma de pago
- Otros (libre)

## 4. OPERACIONES Y COMANDOS

### 4.1 Commands Requeridos
| Command | Estado | Acción Requerida |
|---------|--------|------------------|
| CreateNotaDebitoVentaCommand | ❌ | Crear command específico |
| CreateNotaDebitoVentaCommandValidator | ❌ | Crear validator específico |
| CreateNotaDebitoVentaCommandHandler | ❌ | Crear handler específico |
| EmitirNotaDebitoVentaCommand | ❌ | Reutilizar EmitirComprobanteCommand |
| AnularNotaDebitoVentaCommand | ❌ | Reutilizar AnularComprobanteCommand |
| VincularNotaDebitoCommand | ❌ | Crear command de vinculación |
| DesvincularNotaDebitoCommand | ❌ | Crear command de desvinculación |
| ImportarNotasDebitoCommand | ❌ | Crear command de importación |
| ExportarNotasDebitoCommand | ❌ | Crear command de exportación |

### 4.2 Queries Requeridas
| Query | Estado | Acción Requerida |
|-------|--------|------------------|
| GetNotasDebitoPagedQuery | ❌ | Crear query específica con filtros |
| GetNotaDebitoDetalleQuery | ❌ | Reutilizar GetComprobanteDetalleQuery |
| GetNotasDebitoByClienteQuery | ❌ | Crear query por cliente |
| GetNotasDebitoByOrigenQuery | ❌ | Crear query por documento origen |
| GetNotasDebitoPendientesQuery | ❌ | Crear query por estado |
| GetMotivosDebitoQuery | ❌ | Crear query de catálogo |

## 5. VALIDACIONES DE NEGOCIO ESPECÍFICAS

### 5.1 Validaciones de Creación
- [x] Validar que el cliente esté activo
- [x] Validar que el punto de facturación esté habilitado
- [ ] Validar que el motivo de débito sea válido y activo
- [ ] Si el motivo requiere documento origen, validar que exista
- [ ] Validar que el documento origen sea facturable (no anulado)
- [ ] Validar que el monto del débito no exceda el saldo del documento origen (si aplica)
- [ ] Validar que la moneda sea compatible con el documento origen
- [ ] Validar límites de autorización según el usuario
- [ ] Validar que exista numeración disponible

### 5.2 Validaciones de Items
- [ ] Validar que los items existan
- [ ] Validar que las cantidades sean positivas
- [ ] Validar que los precios sean coherentes
- [ ] Si hay documento origen, validar coherencia con items originales
- [ ] Validar alícuotas IVA según configuración fiscal del cliente

### 5.3 Validaciones de Emisión
- [x] Validar estado del comprobante (debe estar en borrador)
- [x] Validar que el comprobante tenga items
- [x] Validar totales correctos
- [ ] Validar autorización fiscal si corresponde
- [ ] Validar límites de crédito del cliente
- [ ] Validar ventana de cobranza del cliente

## 6. SERVICIOS AUXILIARES

### 6.1 NotaDebitoWorkflowService
| Método | Estado | Acción Requerida |
|--------|--------|------------------|
| CrearBorradorDesdeFactura | ❌ | Crear método |
| ValidarMotivo | ❌ | Crear método |
| CalcularTotales | ✅ | Reutilizar ComprobanteService |
| VincularConOrigen | ❌ | Crear método |
| GenerarAsiento | ✅ | Reutilizar AsientoService |
| ActualizarCuentaCorriente | ✅ | Reutilizar CuentaCorrienteService |
| NotificarCliente | ❌ | Crear método |

### 6.2 NotaDebitoValidationService
| Método | Estado | Acción Requerida |
|--------|--------|------------------|
| ValidarMotivoRequerido | ❌ | Crear método |
| ValidarDocumentoOrigen | ❌ | Crear método |
| ValidarMontos | ❌ | Crear método |
| ValidarAutorizacion | ❌ | Crear método |
| ValidarLimiteCredito | ❌ | Crear método |

## 7. ENDPOINTS DE API

### 7.1 Endpoints Nuevos Requeridos
```
POST   /api/comprobantes/notas-debito                  - Crear borrador de ND
POST   /api/comprobantes/notas-debito/{id}/emitir      - Emitir ND
POST   /api/comprobantes/notas-debito/{id}/anular      - Anular ND
GET    /api/comprobantes/notas-debito                  - Listar NDs paginado
GET    /api/comprobantes/notas-debito/{id}             - Obtener detalle de ND
POST   /api/comprobantes/notas-debito/{id}/vincular    - Vincular ND con origen
DELETE /api/comprobantes/notas-debito/{id}/vincular    - Desvincular ND
POST   /api/comprobantes/notas-debito/importar         - Importar NDs masivo
POST   /api/comprobantes/notas-debito/exportar         - Exportar NDs
GET    /api/comprobantes/notas-debito/motivos          - Obtener catálogo de motivos
```

### 7.2 Endpoints Reutilizables
```
GET    /api/comprobantes/{id}                          - Ya existe, reutilizar
POST   /api/comprobantes/{id}/imprimir                 - Ya existe, reutilizar
GET    /api/comprobantes                               - Ya existe, agregar filtros
```

## 8. FILTROS Y BÚSQUEDAS

### 8.1 Filtros en Listado
| Filtro | Requerido | Estado | Acción Requerida |
|--------|-----------|--------|------------------|
| Por fecha desde/hasta | ✅ | ✅ | Ya existe |
| Por cliente | ✅ | ✅ | Ya existe |
| Por vendedor | ✅ | ✅ | Ya existe |
| Por sucursal | ✅ | ✅ | Ya existe |
| Por punto de facturación | ✅ | ✅ | Ya existe |
| Por número | ✅ | ✅ | Ya existe |
| Por estado | ✅ | ✅ | Ya existe |
| Por motivo de débito | ✅ | ❌ | Agregar filtro |
| Por documento origen | ✅ | ❌ | Agregar filtro |
| Por monto desde/hasta | ✅ | ❌ | Agregar filtro |
| Por estado fiscal (AFIP/SIFEN) | ✅ | ✅ | Ya existe |

## 9. REPORTES E IMPRESIÓN

### 9.1 Reportes Requeridos
| Reporte | Estado | Acción Requerida |
|---------|--------|------------------|
| Nota de débito (ticket) | ❌ | Crear template |
| Nota de débito (A4) | ❌ | Crear template |
| Listado de notas de débito | ❌ | Crear template |
| Resumen por motivo | ❌ | Crear template |
| Resumen por cliente | ❌ | Reutilizar template genérico |
| Resumen por vendedor | ❌ | Reutilizar template genérico |

## 10. INTEGRACIÓN CON OTROS MÓDULOS

### 10.1 Cuenta Corriente
- [x] Registrar débito en cuenta corriente del cliente
- [x] Actualizar saldo del comprobante origen si aplica
- [x] Generar movimiento de cuenta corriente
- [x] Actualizar aging

### 10.2 Contabilidad
- [x] Generar asiento contable
- [x] Débito: Deudores por ventas
- [x] Crédito: Ventas / Otro según motivo

### 10.3 Imputaciones
- [ ] Permitir imputar ND contra facturas
- [ ] Permitir imputar cobros contra ND
- [ ] Mantener trazabilidad de imputaciones

### 10.4 AFIP / SIFEN
- [x] Enviar a autorización fiscal si corresponde
- [x] Obtener CAE/CDC
- [x] Actualizar estado fiscal
- [x] Reenviar en caso de error

## 11. MIGRACIONES DE BASE DE DATOS

### 11.1 Nuevas Entidades
```sql
CREATE TABLE motivos_debito (
    id BIGSERIAL PRIMARY KEY,
    codigo VARCHAR(20) NOT NULL UNIQUE,
    descripcion VARCHAR(200) NOT NULL,
    es_fiscal BOOLEAN NOT NULL DEFAULT false,
    requiere_documento_origen BOOLEAN NOT NULL DEFAULT true,
    afecta_cuenta_corriente BOOLEAN NOT NULL DEFAULT true,
    activo BOOLEAN NOT NULL DEFAULT true,
    fecha_creacion TIMESTAMP NOT NULL DEFAULT NOW(),
    fecha_modificacion TIMESTAMP
);
```

### 11.2 Extensiones a Comprobante
```sql
ALTER TABLE comprobantes 
ADD COLUMN motivo_debito_id BIGINT REFERENCES motivos_debito(id),
ADD COLUMN motivo_debito_observacion TEXT,
ADD COLUMN fecha_anulacion TIMESTAMP,
ADD COLUMN usuario_anulacion_id BIGINT REFERENCES usuarios(id),
ADD COLUMN motivo_anulacion VARCHAR(500);
```

### 11.3 Extensiones a ComprobanteItem
```sql
ALTER TABLE comprobantes_items
ADD COLUMN cantidad_documento_origen DECIMAL(18, 4),
ADD COLUMN precio_documento_origen DECIMAL(18, 4);
```

## 12. PLAN DE IMPLEMENTACIÓN

### Fase 1: Base de Datos y Entidades (Prioridad Alta)
1. Crear entidad `MotivoDebito`
2. Extender `Comprobante` con campos de débito
3. Extender `ComprobanteItem` con campos de referencia origen
4. Crear migraciones
5. Actualizar configuraciones EF Core

### Fase 2: DTOs y Mappings (Prioridad Alta)
1. Extender `ComprobanteDto` con campos de débito
2. Extender `ComprobanteItemDto` con referencias origen
3. Crear `MotivoDebitoDto`
4. Actualizar mappings en AutoMapper

### Fase 3: Commands y Validators (Prioridad Alta)
1. Crear `CreateNotaDebitoVentaCommand`
2. Crear `CreateNotaDebitoVentaCommandValidator`
3. Crear `CreateNotaDebitoVentaCommandHandler`
4. Crear `VincularNotaDebitoCommand`
5. Crear validadores específicos de negocio

### Fase 4: Queries (Prioridad Media)
1. Crear `GetNotasDebitoPagedQuery`
2. Crear `GetNotasDebitoByClienteQuery`
3. Crear `GetNotasDebitoByOrigenQuery`
4. Crear `GetMotivosDebitoQuery`
5. Extender filtros existentes

### Fase 5: Servicios Auxiliares (Prioridad Media)
1. Crear `NotaDebitoWorkflowService`
2. Crear `NotaDebitoValidationService`
3. Integrar con servicios existentes

### Fase 6: API y Controller (Prioridad Alta)
1. Extender `ComprobantesController` con endpoints de ND
2. Crear documentación Swagger
3. Agregar filtros específicos

### Fase 7: Tests (Prioridad Alta)
1. Tests unitarios de commands
2. Tests unitarios de validators
3. Tests unitarios de queries
4. Tests de integración de workflow
5. Tests de servicios

### Fase 8: Importación/Exportación (Prioridad Baja)
1. Crear `ImportarNotasDebitoCommand`
2. Crear `ExportarNotasDebitoCommand`
3. Validaciones masivas
4. Templates de importación

### Fase 9: Reportes (Prioridad Media)
1. Template de impresión ticket
2. Template de impresión A4
3. Reportes de análisis

## 13. CRITERIOS DE ACEPTACIÓN

Una Nota de Débito estará completa cuando:

1. ✅ Se puede crear un borrador con todos los campos de zuluApp
2. ✅ Se puede vincular a un documento origen (factura)
3. ✅ Se valida el motivo y se registra correctamente
4. ✅ Se calcula correctamente el impacto en cuenta corriente
5. ✅ Se emite fiscalmente (AFIP/SIFEN) si corresponde
6. ✅ Se genera el asiento contable correspondiente
7. ✅ Se puede listar con todos los filtros de zuluApp
8. ✅ Se puede imprimir/exportar
9. ✅ Se mantiene auditoría completa de operaciones
10. ✅ Se puede anular con trazabilidad

## 14. NOTAS TÉCNICAS

### Diferencias entre Nota de Débito y Nota de Crédito
- **Nota de Débito**: Incrementa la deuda del cliente (signo positivo)
- **Nota de Crédito**: Disminuye la deuda del cliente (signo negativo)

Ambas comparten la misma estructura base de `Comprobante`, pero:
- Diferentes `TipoComprobante` (catálogo)
- Diferentes motivos (catálogo específico)
- Diferente impacto en cuenta corriente (signo)
- Diferentes validaciones de negocio

### Reutilización de Código
La implementación debe maximizar la reutilización:
- Usar `Comprobante` y `ComprobanteItem` existentes
- Reutilizar servicios de cálculo, fiscales y contables
- Diferenciar por `TipoComprobante` en lugar de crear entidades separadas
- Usar estrategia de validación común con reglas específicas por tipo

### Performance
- Índices en `motivo_debito_id`
- Índice en `comprobante_origen_id`
- Consultas paginadas obligatorias
- Lazy loading en relaciones complejas

## 15. RESUMEN DE GAPS

### Gaps Críticos (Bloquean funcionalidad)
1. ❌ Entidad `MotivoDebito`
2. ❌ Campo `MotivoDebitoId` en `Comprobante`
3. ❌ Commands específicos de ND
4. ❌ Queries específicas de ND
5. ❌ Validaciones de negocio de ND

### Gaps Importantes (Afectan UX)
1. ❌ Campos de referencia a documento origen en DTOs
2. ❌ Filtros específicos de ND
3. ❌ Servicios de workflow de ND
4. ❌ Templates de impresión

### Gaps Menores (Mejoras)
1. ❌ Importación/Exportación
2. ❌ Reportes analíticos
3. ❌ Campos de auditoría extendidos (fecha/usuario anulación)

## 16. SIGUIENTE PASO

**Comenzar por Fase 1**: Crear la base de datos y entidades necesarias para soportar notas de débito con todos sus campos y relaciones.
