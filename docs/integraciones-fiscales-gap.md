# Gap de Integraciones Fiscales

## Objetivo

Documentar con precision la diferencia entre las integraciones fiscales reales del
legacy `zuluApp` y lo que actualmente existe en `ZuluIA_Back`, para convertir el
gap en backlog implementable.

## Conclusión ejecutiva

Hoy `ZuluIA_Back` ya tiene integracion real con AFIP WSFE para:

- solicitud real de CAEA,
- autenticacion WSAA,
- cache de credenciales WSAA,
- solicitud real de CAE,
- emision automatica de CAE en el flujo de comprobantes online,
- auditoria funcional de solicitudes y resultados de CAE,
- persistencia de metadatos AFIP de CAEA.

El gap fiscal principal ya no esta en AFIP WSFE base sino en:

- trazabilidad completa del CAE automatico antes de persistencia del comprobante,
- alineacion entre el envio AFIP y el nuevo desglose persistido de tributos/percepciones,
- integraciones Paraguay/SIFEN/SET,
- integraciones con controladores fiscales Epson/Hasar,
- flujos equivalentes a Hechauka y monitores operativos legacy.

## Evidencia del legacy `zuluApp`

Del relevamiento de archivos del legacy surgen artefactos concretos de integracion:

### AFIP / WSFE / CAE / CAEA

- `CLASES\\clsWSFEBridge.cls`
- `CLASES\\clsWSFEConfig.cls`
- `CLASES\\clsWSFEConnectionRequestResponse.cls`
- `CLASES\\clsWSFEMessageEncoder.cls`
- `CLASES\\clsWSFEResponse.cls`
- `CLASES\\cls_WSFE_cae.cls`
- `CLASES\\cls_WSFE_caea.cls`
- `CLASES\\cls_WSFE_caea_comprobante.cls`
- `CLASES\\cls_WSFE_cae_comprobante.cls`
- `CLASES\\cls_WSFE_XML.cls`
- `FORMULARIOS\\frmActualizarCAE.frm`
- `FORMULARIOS\\frmConsultasCAE.frm`
- `FORMULARIOS\\frmSolicitarCAE.frm`
- `FORMULARIOS\\frm_CAEALst.frm`
- `ASP\\FACTURASCARGARCAE_Listado.asp`

### Paraguay / SIFEN / SET / Timbrado

- `CLASES\\clsFE_Envio_Sifen.cls`
- `CLASES\\clsSIFEN_Distritos.cls`
- `CLASES\\clsSIFEN_Localidades.cls`
- `CLASES\\clsSIFEN_NotaRemision.cls`
- `FORMULARIOS\\frmSIFEN_DatosNotaRemision.frm`
- `FORMULARIOS\\frmSIFEN_Monitor.frm`

### Controladores fiscales / impresoras

- `CLASES\\clsComprobanteEpson.cls`
- `CLASES\\clsImpresoraEpson.cls`
- `CLASES\\clsFiscalHasarSegundaGeneracion.cls`
- `FORMULARIOS\\frmEpsonFiscal.frm`
- `FORMULARIOS\\frmHasarFiscalSegundaGeneracion.frm`
- `FORMULARIOS\\frmPrinterEpson.frm`

### Otros flujos fiscales / exportacion

- `FORMULARIOS\\frmFacturaElectronica.frm`
- `FORMULARIOS\\frmHechaukaVta.frm`
- `MODULOS\\Fiscal.bas`
- reportes CAE/CAEA en carpeta `REPORTES\\`

## Estado actual en `ZuluIA_Back`

### Lo que si existe

#### 1. Configuracion fiscal por punto de facturacion

Existe modelado y CRUD de configuracion fiscal:

- [src/ZuluIA_Back.Domain/Entities/Facturacion/ConfiguracionFiscal.cs](src/ZuluIA_Back.Domain/Entities/Facturacion/ConfiguracionFiscal.cs)
- [src/ZuluIA_Back.Api/Controllers/PuntosFacturacionController.cs](src/ZuluIA_Back.Api/Controllers/PuntosFacturacionController.cs)
- [src/ZuluIA_Back.Application/Features/Facturacion/Commands/PuntoFacturacionMaintenanceCommands.cs](src/ZuluIA_Back.Application/Features/Facturacion/Commands/PuntoFacturacionMaintenanceCommands.cs)

Esto cubre:

- tecnologia
- interfaz fiscal
- puerto
- copias
- directorios
- timers
- flag online

Diagnostico: `configuracion presente`, `integracion real ausente`.

#### 2. Timbrado Paraguay

Existe modelado y lifecycle de timbrados:

- [src/ZuluIA_Back.Domain/Entities/Facturacion/Timbrado.cs](src/ZuluIA_Back.Domain/Entities/Facturacion/Timbrado.cs)
- [src/ZuluIA_Back.Api/Controllers/TimbradoController.cs](src/ZuluIA_Back.Api/Controllers/TimbradoController.cs)

Diagnostico: `timbrado administrativo presente`, `flujo SIFEN/SET real ausente`.

#### 3. CAEA

Existe entidad, comandos, controller y adaptador real AFIP:

- [src/ZuluIA_Back.Domain/Entities/Facturacion/Caea.cs](src/ZuluIA_Back.Domain/Entities/Facturacion/Caea.cs)
- [src/ZuluIA_Back.Api/Controllers/CaeaController.cs](src/ZuluIA_Back.Api/Controllers/CaeaController.cs)
- [src/ZuluIA_Back.Application/Features/Caea/Commands/CaeaCommands.cs](src/ZuluIA_Back.Application/Features/Caea/Commands/CaeaCommands.cs)
- [src/ZuluIA_Back.Infrastructure/Services/AfipWsfeCaeaService.cs](src/ZuluIA_Back.Infrastructure/Services/AfipWsfeCaeaService.cs)

Hoy conviven dos flujos:

- `SolicitarCaeaCommand`: alta manual interna.
- `SolicitarCaeaAfipCommand`: solicitud real a AFIP WSFE.

Ademas, ahora se persisten `FechaProcesoAfip` y `FechaTopeInformarAfip` devueltas
por AFIP.

Diagnostico: `integracion AFIP real presente`, `observabilidad/auditoria CAEA aun parcial`.

#### 4. Emision de CAE en comprobantes

Existe asignacion manual, solicitud explicita a AFIP y emision automatica:

- [src/ZuluIA_Back.Api/Controllers/ComprobantesController.cs](src/ZuluIA_Back.Api/Controllers/ComprobantesController.cs)
- [src/ZuluIA_Back.Application/Features/Comprobantes/Commands/ComprobanteMaintenanceCommands.cs](src/ZuluIA_Back.Application/Features/Comprobantes/Commands/ComprobanteMaintenanceCommands.cs)
- [src/ZuluIA_Back.Application/Features/Comprobantes/Commands/EmitirComprobanteCommandHandler.cs](src/ZuluIA_Back.Application/Features/Comprobantes/Commands/EmitirComprobanteCommandHandler.cs)
- [src/ZuluIA_Back.Application/Features/Comprobantes/Services/AfipCaeComprobanteService.cs](src/ZuluIA_Back.Application/Features/Comprobantes/Services/AfipCaeComprobanteService.cs)
- [src/ZuluIA_Back.Infrastructure/Services/AfipWsfeCaeService.cs](src/ZuluIA_Back.Infrastructure/Services/AfipWsfeCaeService.cs)

El backend ya puede:

- solicitar CAE real a AFIP,
- mapear IVA y tributos para AFIP,
- emitir automaticamente CAE cuando la configuracion fiscal del punto/tipo esta en `Online`,
- registrar auditoria de solicitud, exito y rechazo AFIP para comprobantes ya persistidos,
- persistir el detalle de IVA en `comprobantes_impuestos`,
- persistir el detalle de tributos/percepciones en `comprobantes_tributos`.

Diagnostico: `emision AFIP real presente`, `persistencia previa a auditoria automatica aun pendiente`.

#### 5. WSAA

Existe autenticacion automatica WSAA y cache de credenciales:

- [src/ZuluIA_Back.Infrastructure/Services/AfipWsaaAuthService.cs](src/ZuluIA_Back.Infrastructure/Services/AfipWsaaAuthService.cs)
- [src/ZuluIA_Back.Infrastructure/Services/CachedAfipWsaaAuthService.cs](src/ZuluIA_Back.Infrastructure/Services/CachedAfipWsaaAuthService.cs)

Tambien existe validacion de expiracion para credenciales manuales `Token`/`Sign`
mediante `TokenExpiration`.

Diagnostico: `autenticacion AFIP automatizada presente`.

#### 6. Exportaciones fiscales TXT

Existe un bloque migrado y util:

- [src/ZuluIA_Back.Api/Controllers/ExportacionesFiscalesController.cs](src/ZuluIA_Back.Api/Controllers/ExportacionesFiscalesController.cs)

Esto cubre exportaciones tipo CITI, IIBB y retenciones.

Diagnostico: `migrado`.

## Lo que no aparece en `ZuluIA_Back`

No hay evidencia suficiente de:

1. Clientes o adaptadores para SIFEN/SET.
2. Integracion con impresoras/controladores Epson.
3. Integracion con fiscal Hasar de segunda generacion.
4. Flujo Hechauka equivalente.
5. Monitores operativos fiscales equivalentes al legacy.
6. Auditoria/trace equivalente para CAEA y para el tramo automatico pre-persistencia de CAE.
7. Consumo consistente del desglose persistido de tributos/percepciones en todo el flujo AFIP.

La capa de infraestructura fiscal hoy ya incluye servicios especializados:

- [src/ZuluIA_Back.Infrastructure/Services/AfipWsaaAuthService.cs](src/ZuluIA_Back.Infrastructure/Services/AfipWsaaAuthService.cs)
- [src/ZuluIA_Back.Infrastructure/Services/CachedAfipWsaaAuthService.cs](src/ZuluIA_Back.Infrastructure/Services/CachedAfipWsaaAuthService.cs)
- [src/ZuluIA_Back.Infrastructure/Services/AfipWsfeCaeService.cs](src/ZuluIA_Back.Infrastructure/Services/AfipWsfeCaeService.cs)
- [src/ZuluIA_Back.Infrastructure/Services/AfipWsfeCaeaService.cs](src/ZuluIA_Back.Infrastructure/Services/AfipWsfeCaeaService.cs)

## Brecha exacta por integracion

| Integracion legacy | Estado actual en ZuluIA_Back | Estado |
|---|---|---|
| WSFE / AFIP emision CAE | solicitud real + emision automatica online + auditoria parcial | Parcial avanzado |
| WSFE / AFIP solicitud CAEA | solicitud real + persistencia de metadatos AFIP | Parcial avanzado |
| WSAA / autenticacion AFIP | login CMS real + cache + validacion de expiracion manual | Migrado |
| SIFEN / SET | timbrado administrativo, sin conector visible | Faltante |
| Epson fiscal | solo configuracion fiscal generica | Faltante |
| Hasar fiscal | solo configuracion fiscal generica | Faltante |
| Hechauka | no aparece modulo equivalente visible | Faltante |
| Exportaciones fiscales TXT | existe controller y handlers | Migrado |

## Implicancia para la afirmacion "tiene todo y mas"

Todavia no es honesto decir que `ZuluIA_Back` ya tiene todo lo que tenia
`zuluApp` en materia fiscal, pero la razon ya no es AFIP base. El backend nuevo
ya resuelve operacion AFIP real; lo que sigue faltando esta concentrado en otras
integraciones y en cerrar observabilidad/paridad operativa del flujo fiscal.

## Backlog implementable derivado

### Prioridad 1

1. Resolver trazabilidad completa del CAE automatico antes de persistencia.
2. Consumir el detalle persistido de tributos/percepciones tambien en la construccion del payload AFIP.
3. Completar auditoria funcional equivalente para CAEA.

### Prioridad 2

4. Implementar adaptador SIFEN/SET o flujo Paraguay equivalente.
5. Implementar soporte operativo para timbrado + emision Paraguay.
6. Definir integracion de controladores fiscales Epson/Hasar o decidir explicitamente que quedan fuera de alcance.

### Prioridad 3

7. Agregar tests de integracion para adaptadores fiscales.
8. Agregar tests HTTP para endpoints fiscales.
9. Incorporar monitores/consultas operativas equivalentes para soporte fiscal.

## Criterio de cierre del punto

Este punto se podra considerar realmente cerrado cuando:

1. AFIP WSFE quede completo tambien en trazabilidad y soporte operativo;
2. las integraciones Paraguay/SIFEN/SET tengan definicion e implementacion real o descarte formal;
3. las integraciones con controladores fiscales legacy queden migradas o descartadas explicitamente;
4. haya tests de integracion y documentacion final por proveedor.