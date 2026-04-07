# Paridad funcional: Ajustes de ventas vs `zuluApp`

## Objetivo
Relevar la paridad funcional backend necesaria para reemplazar los formularios legacy de ajustes de ventas de `C:\Zulu\zuluApp` con APIs utilizables desde `C:\Zulu\ZuluIA_Front`.

## Formularios legacy relevados
- `frmAjustesVentaCredito.frm`
- `frmAjustesVentaDebito.frm`
- `frmAjustesPV.frm`

## Hallazgos del legacy

### 1. Ajustes de crédito y débito comparten la misma estructura base
Ambos formularios comparten prácticamente la misma cabecera y el mismo detalle operativo:
- cliente por `legajo`
- sucursal del cliente
- razón social cliente y sucursal
- provincia, localidad, calle, CP, teléfono, CUIT, ingresos brutos e IVA
- tipo de comprobante
- punto de facturación
- número y fecha
- fecha de vencimiento
- condición de venta
- vendedor
- observación general
- descuento global por monto y porcentaje
- grilla de ítems con observación por renglón
- totales: subtotal, neto gravado, neto no gravado, IVA RI, IVA RNI y total

### 2. Diferencia funcional principal entre crédito y débito
La diferencia funcional más clara en la persistencia legacy es el signo del movimiento:
- `frmAjustesVentaCredito` guarda `DebeHaber = -1`
- `frmAjustesVentaDebito` guarda `DebeHaber = 1`

En otras palabras, el formulario no cambia demasiado en UI; cambia sobre todo el impacto comercial/contable.

### 3. Campos legacy detectados en grabación del comprobante
Del guardado VB6 se relevaron estos datos explícitos:
- `nroComprobante`
- `NroComprobanteSucursal`
- `ID_Persona`
- `id_sucursal`
- `razonSocial`
- `domicilio`
- `id_localidad`
- `CondicionIVA`
- `CUIT_CUIL`
- `id_condicion`
- `id_Vendedor`
- `id_cobrador`
- `id_transporte`
- `FechaComprobante`
- `importeIVARI`
- `ImporteIVARNI`
- `NetoGravado`
- `NetoNoGravado`
- `totalComprobante`
- `DebeHaber`
- `id_moneda`
- `cotizacion`
- `PercepcionIVA`
- `PercepcionIngresosBrutos`
- `PercepcionGanancias`
- `RetencionIVA`
- `RetencionesIngresosBrutos`
- `RetencionesGanancias`
- `Municipalidad`
- `Descuento`
- `PorcentajeDescuento`
- `Impuesto`, `Impuesto1`, `Impuesto2`, `Impuesto3`
- `estado`
- `NroCierre`
- `observacion`
- `prefijo`
- `PeriodoIVA`
- `Habilitada`
- `Saldo`
- `Anulada`

### 4. Campos legacy detectados en detalle de renglones
La grilla legacy persiste:
- item
- descripción
- observación del renglón
- costo
- cantidad
- cantidad bonificada
- importe
- precio de venta
- porcentaje IVA
- porcentaje de descuento
- total de línea
- descuento general aplicado
- orden de renglón

## Estado backend actual

### Cobertura ya presente
El backend ya tiene una base reutilizable importante para ajustes/documentos de ventas:
- `CrearBorradorVentaCommand`
- `EmitirDocumentoVentaCommand`
- `VentasController`
- `Comprobante` con datos comerciales, logísticos y fiscales extendidos
- `ComprobanteItem` con lote, serie, vencimiento, unidad de medida, observación de renglón, precio lista original, comisión de vendedor y referencia de ítem origen
- `MotivoDebitoId` y `MotivoDebitoObservacion` ya existen en la entidad `Comprobante`
- `MotivoDebito` ya existe como catálogo de referencia

### Cobertura agregada en este slice
Se completó la lectura del detalle de comprobantes para exponer mejor los datos que necesita el front de ajustes/notas:
- `MotivoDebitoId`
- `MotivoDebitoDescripcion`
- `MotivoDebitoObservacion`
- `MotivoDebitoEsFiscal`
- `CreatedByUsuario`
- `UpdatedByUsuario`
- `FechaAnulacion`
- `UsuarioAnulacionId`
- `UsuarioAnulacionNombre`
- `MotivoAnulacion`
- `ComprobanteOrigenNumero`
- `ComprobanteOrigenTipo`
- `ComprobanteOrigenFecha`
- `CantidadDocumentoOrigen` por renglón
- `PrecioDocumentoOrigen` por renglón
- `UnidadMedidaDescripcion` por renglón
- `VendedorNombre`, `VendedorLegajo`
- `CobradorNombre`, `CobradorLegajo`
- `TransporteRazonSocial`

## Gaps backend todavía abiertos

### 1. Commands/endpoints específicos de ajustes
Todavía no existe una capa explícita orientada a:
- `CreateAjusteVentaCreditoCommand`
- `CreateAjusteVentaDebitoCommand`
- endpoints dedicados en `VentasController` para distinguir semánticamente crédito vs débito
- validación explícita del signo/impacto comercial equivalente al `DebeHaber` del legacy

### 2. Hidratación automática de datos comerciales al crear borrador
El legacy completa varios datos desde cliente/sucursal/contacto al momento de grabar. Falta validar si el backend moderno debe autocompletar siempre:
- vendedor
- cobrador
- condición de venta
- domicilio snapshot completo
- transporte relacionado
- plazo de vencimiento por cliente

### 3. Paridad de `frmAjustesPV`
`frmAjustesPV` parece cubrir parametrización de puntos/comprobantes de venta:
- prefijos
- máscara
- tipo de comprobante
- tipo de punto de venta
- moneda
- impresiones
- fiscalidad/configuración

Ese formulario todavía requiere relevamiento puntual contra:
- `PuntosFacturacionController`
- configuración fiscal asociada
- numeradores y permisos operativos

### 4. Equivalente explícito de `DebeHaber`
El backend actual maneja estados, totales y cuenta corriente, pero no expone todavía un campo/semántica explícita equivalente al `DebeHaber` legacy para distinguir ajustes débito/crédito de manera inequívoca en DTOs y reglas.

## Próximos pasos recomendados
1. Crear endpoints dedicados para `ajustes-credito` y `ajustes-debito` en `VentasController`.
2. Definir la regla backend que reemplace formalmente `DebeHaber`.
3. Autocompletar datos comerciales desde cliente/sucursal al crear el borrador de ajuste.
4. Relevar `frmAjustesPV` contra puntos de facturación y configuración fiscal.
5. Incorporar tests de query/DTO para asegurar que los campos visibles del formulario legacy viajen completos al frontend.
