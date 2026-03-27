# Paridad Funcional ZuluApp vs ZuluIA_Back

## Objetivo

Este documento deja una matriz operativa para completar la migracion funcional
desde `d:\Zulu\zuluApp` hacia `d:\Zulu\ZuluIA_Back`.

El criterio no es copiar pantalla por pantalla. El backend nuevo responde a un
frontend nuevo, asi que la pregunta correcta es esta:

- que logica real del legacy ya esta cubierta,
- que fue absorbido con otro nombre o mejor diseño,
- que sigue faltando como backend real,
- y que era solo UI, reporte o tooling auxiliar del monolito.

## Estado actual resumido

- `ZuluIA_Back` ya supera al legacy en arquitectura, mantenibilidad, testing y
  capacidad de evolucion.
- La migracion funcional del core esta avanzada, pero no cerrada.
- La brecha restante ya no esta principalmente en CRUD basico, sino en
  integraciones, submodulos administrativos, procesos operativos heredados y
  logica aun escondida en ASP/VB6/SQL.

## Evidencia relevada

- `ZuluIA_Back`: 95 controllers API visibles.
- `zuluApp`: 1550 archivos ASP.
- `zuluApp`: 789 nombres ASP unicos.
- `zuluApp`: 375 prefijos/modulos legacy aproximados.
- `zuluApp`: 559 archivos VB6 (`.bas`, `.cls`, `.frm`, `.ctl`, `.vbp`).
- `zuluApp`: 72 archivos SQL en carpeta `SQL`.

Estas cifras no implican que falten 280 modulos backend. Gran parte del legacy
duplica un mismo caso de uso en variantes `Listado`, `InsertarForm`, `InsertarDB`,
`EditarForm`, `EditarDB`, `Eliminar`, etc. Aun asi, la diferencia de superficie
sigue siendo suficiente para no dar por cerrada la paridad.

## Criterio de estados

- `Superado`: la logica existe y la solucion nueva es tecnicamente mejor.
- `Migrado`: hay evidencia clara de paridad backend suficiente.
- `Parcial`: existe cobertura importante, pero faltan subflujos o cierre de
  equivalencia operativa.
- `Sin match claro`: no hay API visible suficiente o el dominio sigue ambiguo.
- `Probable UI/reporte`: el legado muestra pantallas o tooling, pero no hay
  evidencia de backend productivo imprescindible.

## Matriz maestra de paridad

| Modulo legacy | Evidencia en zuluApp | Cobertura en ZuluIA_Back | Estado | Lectura actual |
|---|---|---|---|---|
| Autenticacion y sesion | login, sesion, acceso | [src/ZuluIA_Back.Api/Controllers/AuthController.cs](src/ZuluIA_Back.Api/Controllers/AuthController.cs), [src/ZuluIA_Back.Api/Program.cs](src/ZuluIA_Back.Api/Program.cs) | Superado | JWT, middleware y setup moderno superan ampliamente al legado. |
| Menu y navegacion | `MENU*`, `clsMenu*` | [src/ZuluIA_Back.Api/Controllers/MenuController.cs](src/ZuluIA_Back.Api/Controllers/MenuController.cs), [src/ZuluIA_Back.Api/Controllers/UsuariosController.cs](src/ZuluIA_Back.Api/Controllers/UsuariosController.cs) | Migrado | Existe gestion de arbol, asignacion a usuario y activacion. |
| Usuarios, permisos y relaciones | `USUARIOS*`, `USUARIOSXPERMISOS`, `USUARIOSXUSUARIOS`, `USUARIOSXSUCURSAL*` | [src/ZuluIA_Back.Api/Controllers/UsuariosController.cs](src/ZuluIA_Back.Api/Controllers/UsuariosController.cs), [src/ZuluIA_Back.Api/Controllers/SeguridadController.cs](src/ZuluIA_Back.Api/Controllers/SeguridadController.cs) | Migrado | El backend nuevo cubre permisos, menu, parametros y relaciones entre usuarios. |
| Terceros clientes/proveedores | `CLIENTES*`, `PROVEEDORES*`, personas y vinculaciones | [src/ZuluIA_Back.Api/Controllers/TercerosController.cs](src/ZuluIA_Back.Api/Controllers/TercerosController.cs), [src/ZuluIA_Back.Api/Controllers/CategoriasClientesController.cs](src/ZuluIA_Back.Api/Controllers/CategoriasClientesController.cs), [src/ZuluIA_Back.Api/Controllers/CategoriasProveedoresController.cs](src/ZuluIA_Back.Api/Controllers/CategoriasProveedoresController.cs), [src/ZuluIA_Back.Api/Controllers/EstadosClientesController.cs](src/ZuluIA_Back.Api/Controllers/EstadosClientesController.cs), [src/ZuluIA_Back.Api/Controllers/EstadosProveedoresController.cs](src/ZuluIA_Back.Api/Controllers/EstadosProveedoresController.cs) | Superado | Base de terceros bien migrada y mejor estructurada. |
| Geografia y datos base | `PAISES`, `PROVINCIAS`, `LOCALIDADES`, `BARRIOS`, `JURISDICCIONES` | [src/ZuluIA_Back.Api/Controllers/GeografiaController.cs](src/ZuluIA_Back.Api/Controllers/GeografiaController.cs), [src/ZuluIA_Back.Api/Controllers/JurisdiccionesController.cs](src/ZuluIA_Back.Api/Controllers/JurisdiccionesController.cs), [src/ZuluIA_Back.Api/Controllers/RegionesController.cs](src/ZuluIA_Back.Api/Controllers/RegionesController.cs), [src/ZuluIA_Back.Api/Controllers/ZonasController.cs](src/ZuluIA_Back.Api/Controllers/ZonasController.cs) | Migrado | `GeografiaController` ya cubre lectura y CRUD para paises, provincias, localidades y barrios. Lo pendiente pasa a ser validacion fina contra flujos reales del legacy y no ausencia de superficie API. |
| Configuracion general y variables | `CONFIG*`, `SIS_BUSQUEDA`, variables varias | [src/ZuluIA_Back.Api/Controllers/ConfiguracionController.cs](src/ZuluIA_Back.Api/Controllers/ConfiguracionController.cs), [src/ZuluIA_Back.Api/Controllers/VariablesController.cs](src/ZuluIA_Back.Api/Controllers/VariablesController.cs), [src/ZuluIA_Back.Api/Controllers/BusquedasController.cs](src/ZuluIA_Back.Api/Controllers/BusquedasController.cs) | Migrado | Hay backend visible para configuracion, variables y filtros guardados. |
| Cajas, cuentas y cierres | `CAJAS*`, `CIERRESCAJAS*`, `FORMAPAGOCUENTACAJA`, `CAJASCUENTASBANCARIAS` | [src/ZuluIA_Back.Api/Controllers/CajasController.cs](src/ZuluIA_Back.Api/Controllers/CajasController.cs), [src/ZuluIA_Back.Api/Controllers/BancosController.cs](src/ZuluIA_Back.Api/Controllers/BancosController.cs), [src/ZuluIA_Back.Api/Controllers/CobrosController.cs](src/ZuluIA_Back.Api/Controllers/CobrosController.cs), [src/ZuluIA_Back.Api/Controllers/PagosController.cs](src/ZuluIA_Back.Api/Controllers/PagosController.cs) | Parcial | Caja base y flujos financieros existen, pero no hay match obvio por nombre para toda la administracion de cuentas bancarias por caja y forma de pago. |
| Cheques y chequeras | `CHEQUERAS*`, `FROCHEQUES*` | [src/ZuluIA_Back.Api/Controllers/ChequesController.cs](src/ZuluIA_Back.Api/Controllers/ChequesController.cs), [src/ZuluIA_Back.Api/Controllers/ChequerasController.cs](src/ZuluIA_Back.Api/Controllers/ChequerasController.cs) | Migrado | La superficie backend es visible y el lifecycle operativo principal del cheque ya tiene cobertura API simetrica; falta confirmar solo equivalencia fina de algunos listados operativos del legacy. |
| Cobros, pagos y cuenta corriente | `COBROS*`, `PAGOS*`, resumentes de cuentas | [src/ZuluIA_Back.Api/Controllers/CobrosController.cs](src/ZuluIA_Back.Api/Controllers/CobrosController.cs), [src/ZuluIA_Back.Api/Controllers/PagosController.cs](src/ZuluIA_Back.Api/Controllers/PagosController.cs), [src/ZuluIA_Back.Api/Controllers/RecibosController.cs](src/ZuluIA_Back.Api/Controllers/RecibosController.cs), [src/ZuluIA_Back.Api/Controllers/CuentaCorrienteController.cs](src/ZuluIA_Back.Api/Controllers/CuentaCorrienteController.cs), [src/ZuluIA_Back.Api/Controllers/SeguimientoOrdenPagoController.cs](src/ZuluIA_Back.Api/Controllers/SeguimientoOrdenPagoController.cs) | Parcial | El core existe, pero sigue pendiente validar paridad completa de todas las vistas resumen y operativas heredadas. |
| Contabilidad general | `ASIENTOS*`, `PLANCUENTAS*`, `EJERCICIO*`, `LIBROIVA*`, centros de costo | [src/ZuluIA_Back.Api/Controllers/AsientosController.cs](src/ZuluIA_Back.Api/Controllers/AsientosController.cs), [src/ZuluIA_Back.Api/Controllers/ContabilidadController.cs](src/ZuluIA_Back.Api/Controllers/ContabilidadController.cs), [src/ZuluIA_Back.Api/Controllers/PlanCuentasController.cs](src/ZuluIA_Back.Api/Controllers/PlanCuentasController.cs), [src/ZuluIA_Back.Api/Controllers/EjerciciosController.cs](src/ZuluIA_Back.Api/Controllers/EjerciciosController.cs), [src/ZuluIA_Back.Api/Controllers/PeriodosContablesController.cs](src/ZuluIA_Back.Api/Controllers/PeriodosContablesController.cs), [src/ZuluIA_Back.Api/Controllers/LibroIvaController.cs](src/ZuluIA_Back.Api/Controllers/LibroIvaController.cs), [src/ZuluIA_Back.Api/Controllers/CentrosCostoController.cs](src/ZuluIA_Back.Api/Controllers/CentrosCostoController.cs) | Parcial | La base contable esta migrada; falta verificar reportes, cierres y variantes operativas especificas del legado. |
| Ventas, comprobantes y derivados | `VTA*`, `COMPROBANTES*`, `NOTASPEDIDO*`, `PRESUPUESTOS*` | [src/ZuluIA_Back.Api/Controllers/ComprobantesController.cs](src/ZuluIA_Back.Api/Controllers/ComprobantesController.cs), [src/ZuluIA_Back.Api/Controllers/NotasPedidoController.cs](src/ZuluIA_Back.Api/Controllers/NotasPedidoController.cs), [src/ZuluIA_Back.Api/Controllers/PresupuestosController.cs](src/ZuluIA_Back.Api/Controllers/PresupuestosController.cs), [src/ZuluIA_Back.Api/Controllers/ImputacionesController.cs](src/ZuluIA_Back.Api/Controllers/ImputacionesController.cs) | Parcial | Mucha cobertura real, incluyendo tipos disponibles, saldo pendiente por tercero, estadisticas por tipo e imputacion masiva desde `ComprobantesController`, pero aun no esta cerrada toda la equivalencia operativa del legacy. |
| Compras y abastecimiento | `CPR*`, requisiciones, cotizaciones, ordenes | [src/ZuluIA_Back.Api/Controllers/ComprasControllers.cs](src/ZuluIA_Back.Api/Controllers/ComprasControllers.cs), [src/ZuluIA_Back.Api/Controllers/OrdenesCompraController.cs](src/ZuluIA_Back.Api/Controllers/OrdenesCompraController.cs) | Parcial | Buen avance; requisiciones, ordenes y cotizaciones ya tienen mejor cobertura de lifecycle en API, incluyendo aceptar/rechazar cotizaciones de compra, pero aun falta evidencia plena de todos los estados, habilitaciones y vistas del circuito legacy. |
| Stock, depositos e items | `ITEMS*`, `DEPOSITOS*`, `STK*`, movimientos y ajustes | [src/ZuluIA_Back.Api/Controllers/ItemsController.cs](src/ZuluIA_Back.Api/Controllers/ItemsController.cs), [src/ZuluIA_Back.Api/Controllers/StockController.cs](src/ZuluIA_Back.Api/Controllers/StockController.cs), [src/ZuluIA_Back.Api/Controllers/MovimientosStockController.cs](src/ZuluIA_Back.Api/Controllers/MovimientosStockController.cs), [src/ZuluIA_Back.Api/Controllers/DepositosController.cs](src/ZuluIA_Back.Api/Controllers/DepositosController.cs), [src/ZuluIA_Back.Api/Controllers/ListasPreciosController.cs](src/ZuluIA_Back.Api/Controllers/ListasPreciosController.cs), [src/ZuluIA_Back.Api/Controllers/AtributosController.cs](src/ZuluIA_Back.Api/Controllers/AtributosController.cs) | Parcial | El nucleo esta, pero quedan por confirmar movimientos especiales y reglas heredadas menos visibles. |
| Produccion y formulas | formulas, ordenes, declaraciones | [src/ZuluIA_Back.Api/Controllers/FormulasProduccionController.cs](src/ZuluIA_Back.Api/Controllers/FormulasProduccionController.cs), [src/ZuluIA_Back.Api/Controllers/OrdenesTrabajosController.cs](src/ZuluIA_Back.Api/Controllers/OrdenesTrabajosController.cs), [src/ZuluIA_Back.Api/Controllers/OrdenesPreparacionController.cs](src/ZuluIA_Back.Api/Controllers/OrdenesPreparacionController.cs), [src/ZuluIA_Back.Api/Controllers/OrdenesEmpaqueController.cs](src/ZuluIA_Back.Api/Controllers/OrdenesEmpaqueController.cs) | Parcial | Base muy avanzada; el lifecycle principal de ordenes de trabajo ya tiene cobertura API de inicio, finalizacion y cancelacion, pero falta certificar equivalencia de circuitos especiales. |
| CRM, helpdesk y mesa de entrada | `CRM*`, `MESAENTRADA*`, tickets y seguimientos | [src/ZuluIA_Back.Api/Controllers/CrmController.cs](src/ZuluIA_Back.Api/Controllers/CrmController.cs), [src/ZuluIA_Back.Api/Controllers/HelpdeskController.cs](src/ZuluIA_Back.Api/Controllers/HelpdeskController.cs), [src/ZuluIA_Back.Api/Controllers/MesaEntradaController.cs](src/ZuluIA_Back.Api/Controllers/MesaEntradaController.cs) | Migrado | Hay backend visible y moderno para esos bloques. |
| RRHH | `EMPLEADOS*`, liquidaciones, areas | [src/ZuluIA_Back.Api/Controllers/EmpleadosController.cs](src/ZuluIA_Back.Api/Controllers/EmpleadosController.cs) | Parcial | Hay funcionalidades visibles, pero no alcanza para afirmar cierre total del vertical legacy. |
| Cedulones, miembros y matriculas | `COLEGIO*`, inscripciones, cobranzas sectoriales | [src/ZuluIA_Back.Api/Controllers/CedulonesController.cs](src/ZuluIA_Back.Api/Controllers/CedulonesController.cs), [src/ZuluIA_Back.Api/Controllers/MiembrosController.cs](src/ZuluIA_Back.Api/Controllers/MiembrosController.cs), [src/ZuluIA_Back.Api/Controllers/MatriculasController.cs](src/ZuluIA_Back.Api/Controllers/MatriculasController.cs) | Parcial | El vertical existe por piezas, pero no esta demostrada la equivalencia funcional total. |
| Franquicias, planillas y plan de trabajo | `FRA_*`, evaluaciones, variables, planillas, regiones | [src/ZuluIA_Back.Api/Controllers/PlanTrabajoController.cs](src/ZuluIA_Back.Api/Controllers/PlanTrabajoController.cs), [src/ZuluIA_Back.Api/Controllers/PlanillasController.cs](src/ZuluIA_Back.Api/Controllers/PlanillasController.cs), [src/ZuluIA_Back.Api/Controllers/VariablesController.cs](src/ZuluIA_Back.Api/Controllers/VariablesController.cs), [src/ZuluIA_Back.Api/Controllers/RegionesController.cs](src/ZuluIA_Back.Api/Controllers/RegionesController.cs), [src/ZuluIA_Back.Api/Controllers/GruposEconomicosController.cs](src/ZuluIA_Back.Api/Controllers/GruposEconomicosController.cs), [src/ZuluIA_Back.Api/Controllers/FranquiciasRegionesController.cs](src/ZuluIA_Back.Api/Controllers/FranquiciasRegionesController.cs), [src/ZuluIA_Back.Api/Controllers/FranquiciasVariablesUsuariosController.cs](src/ZuluIA_Back.Api/Controllers/FranquiciasVariablesUsuariosController.cs) | Parcial | El bloque FRA ya cubre planillas, planes, variables, regiones, grupos economicos, asignaciones franquicia-region y variables por usuario. Lo pendiente ya no es un faltante administrativo claro, sino validacion funcional fina contra casos reales del legacy. |
| BI, cubos y dashboards | `CUBOS*`, `BI*`, diseno y visualizacion | [src/ZuluIA_Back.Api/Controllers/CubosController.cs](src/ZuluIA_Back.Api/Controllers/CubosController.cs), [src/ZuluIA_Back.Api/Controllers/ThorController.cs](src/ZuluIA_Back.Api/Controllers/ThorController.cs), [src/ZuluIA_Back.Api/Controllers/RankingsController.cs](src/ZuluIA_Back.Api/Controllers/RankingsController.cs) | Parcial | Hay backend BI, pero no se ve cerrado el tooling equivalente del escritorio legacy. |
| Fiscal y facturacion operativa | timbrado, puntos, CAEA, exportaciones, SIFEN/AFIP | [src/ZuluIA_Back.Api/Controllers/TimbradoController.cs](src/ZuluIA_Back.Api/Controllers/TimbradoController.cs), [src/ZuluIA_Back.Api/Controllers/PuntosFacturacionController.cs](src/ZuluIA_Back.Api/Controllers/PuntosFacturacionController.cs), [src/ZuluIA_Back.Api/Controllers/CaeaController.cs](src/ZuluIA_Back.Api/Controllers/CaeaController.cs), [src/ZuluIA_Back.Api/Controllers/ExportacionesFiscalesController.cs](src/ZuluIA_Back.Api/Controllers/ExportacionesFiscalesController.cs), [src/ZuluIA_Back.Api/Controllers/ComprobantesController.cs](src/ZuluIA_Back.Api/Controllers/ComprobantesController.cs), [src/ZuluIA_Back.Api/Controllers/AuditoriaCaeasController.cs](src/ZuluIA_Back.Api/Controllers/AuditoriaCaeasController.cs), [src/ZuluIA_Back.Api/Controllers/AuditoriaComprobantesController.cs](src/ZuluIA_Back.Api/Controllers/AuditoriaComprobantesController.cs), [src/ZuluIA_Back.Api/Controllers/AutorizacionesComprobantesController.cs](src/ZuluIA_Back.Api/Controllers/AutorizacionesComprobantesController.cs), [src/ZuluIA_Back.Api/Controllers/HabilitacionesComprobantesController.cs](src/ZuluIA_Back.Api/Controllers/HabilitacionesComprobantesController.cs) | Parcial | El backend ya cubre exportaciones fiscales, auditoria, autorizaciones/habilitaciones y operaciones SIFEN por comprobante y por lote. Lo pendiente es validar la paridad fina de integraciones y perifericos legacy. |
| Importadores, procesos batch y monitores | importadores, sincronizacion, monitor de exportacion/PDF | [src/ZuluIA_Back.Api/Controllers/ComprobantesController.cs](src/ZuluIA_Back.Api/Controllers/ComprobantesController.cs), [src/ZuluIA_Back.Api/Controllers/PuntosFacturacionController.cs](src/ZuluIA_Back.Api/Controllers/PuntosFacturacionController.cs), [src/ZuluIA_Back.Api/Controllers/CotizacionesController.cs](src/ZuluIA_Back.Api/Controllers/CotizacionesController.cs) | Parcial | Ya existe batch operativo visible para SIFEN Paraguay, imputacion masiva de comprobantes y tambien importacion masiva de cotizaciones por moneda. Sigue sin verse evidencia fuerte de importadores productivos genericos mas amplios ni de todos los monitores heredados. |
| Reportes, reimpresion y tooling documental | ActiveReports, reimpresion, impresion fiscal, PDFs por lote | [src/ZuluIA_Back.Api/Controllers/ExportacionesFiscalesController.cs](src/ZuluIA_Back.Api/Controllers/ExportacionesFiscalesController.cs), [src/ZuluIA_Back.Api/Controllers/ComprobantesController.cs](src/ZuluIA_Back.Api/Controllers/ComprobantesController.cs), [src/ZuluIA_Back.Api/Controllers/NotasPedidoController.cs](src/ZuluIA_Back.Api/Controllers/NotasPedidoController.cs), [src/ZuluIA_Back.Api/Controllers/PresupuestosController.cs](src/ZuluIA_Back.Api/Controllers/PresupuestosController.cs), [src/ZuluIA_Back.Api/Controllers/CartaPorteController.cs](src/ZuluIA_Back.Api/Controllers/CartaPorteController.cs), [src/ZuluIA_Back.Api/Controllers/ThorController.cs](src/ZuluIA_Back.Api/Controllers/ThorController.cs) | Parcial | Ya hay exportaciones fiscales descargables, CSV operativos, fachada BI y endpoints dedicados de reimpresion para comprobantes, notas de pedido, presupuestos y cartas de porte. El gap pendiente se concentra mas en documentos finales adicionales y tooling documental legacy que en ausencia total de salidas backend. |
| Ayuda y pantallas auxiliares | `AYUDA*`, utilidades varias | sin modulo API equivalente visible | Probable UI/reporte | No parece un gap backend prioritario salvo que el frontend nuevo dependa de contenido administrable. |
| Sorteos y modulos especiales | `SORTEO*` | [src/ZuluIA_Back.Api/Controllers/SorteoListasController.cs](src/ZuluIA_Back.Api/Controllers/SorteoListasController.cs) | Parcial | Existe una pieza del vertical, pero falta confirmar si cubre toda la logica heredada. |
| Tests del backend moderno | no aplica como API en legacy | [tests/ZuluIA_Back.UnitTests](tests/ZuluIA_Back.UnitTests), [tests/ZuluIA_Back.Architecture.Tests](tests/ZuluIA_Back.Architecture.Tests) | Superado | Calidad tecnica superior al legado. |
| Tests de integracion HTTP | no aplica directo en legacy | sin evidencia de `WebApplicationFactory` o `TestServer` en tests | Sin match claro | Sigue siendo deuda fuerte para afirmar reemplazo total con confianza contractual. |

## Gaps prioritarios confirmados o muy probables

### P0 - auditar ya

1. Franquicias administrativas ya cubiertas en API
	- Ya expuestos: `GrupoEconomico`, `FranquiciaXRegion`, `FranquiciaVariableXUsuario`.
	- El gap restante del bloque FRA pasa a ser validacion operativa y de paridad fina, no ausencia de superficie backend.

2. Geografia CRUD completa ya cubierta en API
	- [src/ZuluIA_Back.Api/Controllers/GeografiaController.cs](src/ZuluIA_Back.Api/Controllers/GeografiaController.cs) ya expone GET, POST, PUT y DELETE para paises, provincias, localidades y barrios.
	- El gap restante pasa a ser validacion operativa fina contra el uso real del legacy.

3. Procesos batch, importadores y monitores
	- Ya hay batch visible para SIFEN Paraguay en [src/ZuluIA_Back.Api/Controllers/ComprobantesController.cs](src/ZuluIA_Back.Api/Controllers/ComprobantesController.cs): preview, reintento, conciliacion, resumen y export CSV.
	- Ya hay imputacion masiva y consulta operativa de saldo pendiente por tercero en [src/ZuluIA_Back.Api/Controllers/ComprobantesController.cs](src/ZuluIA_Back.Api/Controllers/ComprobantesController.cs).
	- Ya hay importacion masiva puntual de cotizaciones en [src/ZuluIA_Back.Api/Controllers/CotizacionesController.cs](src/ZuluIA_Back.Api/Controllers/CotizacionesController.cs).
	- El gap real restante parece concentrarse en importadores productivos genericos mas amplios y monitores heredados no fiscales.

4. Reporting y reimpresion documental
	- Ya existe backend para exportaciones fiscales y salidas BI.
	- [src/ZuluIA_Back.Api/Controllers/ComprobantesController.cs](src/ZuluIA_Back.Api/Controllers/ComprobantesController.cs) ya expone estadisticas agregadas por tipo de comprobante para una sucursal y periodo.
	- Ya existe reimpresion dedicada para comprobantes en [src/ZuluIA_Back.Api/Controllers/ComprobantesController.cs](src/ZuluIA_Back.Api/Controllers/ComprobantesController.cs).
	- Ya existe reimpresion dedicada para notas de pedido en [src/ZuluIA_Back.Api/Controllers/NotasPedidoController.cs](src/ZuluIA_Back.Api/Controllers/NotasPedidoController.cs).
	- Ya existe reimpresion dedicada para presupuestos en [src/ZuluIA_Back.Api/Controllers/PresupuestosController.cs](src/ZuluIA_Back.Api/Controllers/PresupuestosController.cs).
	- Ya existe reimpresion dedicada para cartas de porte en [src/ZuluIA_Back.Api/Controllers/CartaPorteController.cs](src/ZuluIA_Back.Api/Controllers/CartaPorteController.cs).
	- El gap real restante parece ser documentos finales adicionales y tooling de escritorio acoplado al legacy.

### P1 - auditar despues de P0

1. Cajas/cuentas bancarias/formas de pago con paridad fina.
2. Circuito completo de compras con habilitaciones y estados legacy.
3. Variantes operativas de ventas/comprobantes no cubiertas solo por CRUD principal.
4. RRHH y verticales sectoriales (`Cedulones`, `Miembros`, `Matriculas`) con foco en casos reales.
5. BI/cubos/reportes gerenciales no triviales.

### P2 - deuda tecnica para reemplazo confiable

1. Suite de integracion HTTP y contrato de API.
2. Menor dependencia de lecturas directas desde controllers en algunos modulos.
3. Matriz de casos reales contra datasets del legacy para validar paridad de resultados.

## Backlog operativo inmediato

1. Validar `FRA_*` contra casos reales del legacy.
	- Ya migrado: `PlanTrabajo`, `Planillas`, `Variables`, `Regiones`, `GrupoEconomico`, `FranquiciaXRegion`, `FranquiciaVariableXUsuario`.
	- Pendiente: confirmar equivalencia operativa fina con uso real del frontend nuevo y del legacy.

2. Relevar `PAISES`, `PROVINCIAS`, `LOCALIDADES`, `BARRIOS` del legacy.
	- Decidir si el frontend nuevo necesita CRUD completo o solo lectura administrada por otro canal.

3. Relevar importadores y procesos batch no fiscales del legacy.
	- Clasificar cada uno como obligatorio, reemplazado externamente o descartable.

4. Relevar reporting/reimpresion.
	- Separar reportes de negocio realmente necesarios de tooling de escritorio no migrable tal cual.

5. Reforzar validacion de reemplazo.
	- Agregar tests de integracion HTTP para endpoints criticos.
	- Comparar salidas de procesos clave contra escenarios reales del legacy.

## Conclusion honesta

Hoy `ZuluIA_Back` ya es mejor backend que `zuluApp` en calidad tecnica y base
arquitectonica.

Pero todavia no es serio afirmar que reemplaza funcionalmente al legacy por
completo. La diferencia restante no esta en el CRUD principal ya visible, sino
en los bordes de negocio y operacion: franquicias administrativas faltantes,
geografia editable, procesos batch/importadores, reportes/reimpresion y la
necesidad de validar la equivalencia con pruebas de integracion y casos reales.

La forma correcta de cerrar esta migracion es seguir este orden:

1. confirmar gaps backend reales,
2. convertirlos en backlog priorizado,
3. implementar por bloques,
4. y validar contra comportamiento legacy donde realmente importe.