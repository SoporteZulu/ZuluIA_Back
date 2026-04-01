# Validación Final - Productos OK Ventas

## Objetivo
Verificar si productos backend quedó en estado `OK ventas` contra la referencia funcional de `C:\Zulu\zuluApp`, sin ampliar alcance administrativo.

---

## OK resuelto

- `GET /api/items` cubre búsqueda comercial por código, código alternativo, código de barras y descripción.
- `GET /api/items` soporta filtros operativos relevantes para ventas: `soloVendibles`, `soloConStock`, `soloProductos`.
- `GET /api/items/vendibles` expone un selector liviano para combos/autocomplete con precio, unidad, descuento máximo, stock disponible y bandera comercial.
- `GET /api/items/{id}/precio` bloquea ítems inactivos o no vendibles y devuelve snapshot comercial de stock.
- `GET /api/items/por-codigo/{codigo}` y `GET /api/items/por-codigo-barras/{codigoBarras}` aceptan `soloVendibles=true` para búsquedas directas de venta.
- DTOs de listado y precio ya exponen `EsVendible` y datos suficientes para decidir uso comercial.

---

## Falta crítica

- Ninguna detectada para el objetivo `OK ventas` backend-first definido en esta ventana.

---

## Falta no crítica

- Endpoint exacto dedicado por código alternativo.
- Consolidar lógica de stock comercial en un servicio compartido para evitar duplicación entre handlers.
- Endurecer eventualmente el detalle `GET /api/items/{id}` con una señal comercial resumida si el frontend decide usarlo en selector, aunque hoy ya existe contrato liviano específico.
- Validación funcional manual contra casos reales de `zuluApp` con datos de negocio en PostgreSQL local.

---

## Cobertura mínima de tests

Corrida focalizada validada:

- `ItemsControllerTests`
- `GetItemsPagedQueryHandlerTests`
- `GetItemPrecioQueryHandlerTests`
- `GetItemsVendiblesQueryHandlerTests`
- `GetItemByIdQueryHandlerTests`

Resultado:

- `49/49 PASS`

Escenarios cubiertos:

- selector comercial vendible
- búsqueda rápida y filtros de ventas
- item inactivo
- item no vendible
- item con stock y cálculo de disponible
- servicio sin stock obligatorio
- resolución de precio desde lista/promoción

---

## Build

Validación final ejecutada:

- `dotnet build d:\Zulu\ZuluIA_Back\ZuluIA_Back.sln`

Resultado:

- `OK`
- queda una advertencia no relacionada preexistente en tests (`RegistrarDevolucionVentaCommandValidatorTests`)

---

## Checklist GO / NO GO

| Check | Estado |
|---|---|
| Búsqueda confiable por código/barra/descripción | GO |
| Selector comercial liviano para ventas | GO |
| Validación de item inactivo/no vendible | GO |
| Precio utilizable para renglón | GO |
| Stock comercial utilizable para renglón | GO |
| Cobertura mínima de escenarios críticos | GO |
| Build solución | GO |

---

## Recomendación final

**GO** para considerar productos backend en estado `OK ventas` dentro de esta ventana.

El residual abierto es de endurecimiento y mantenimiento, no bloquea el uso comercial mínimo para pedidos/remitos/facturas desde backend.