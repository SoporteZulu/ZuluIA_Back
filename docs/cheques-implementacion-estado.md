# Estado de implementación de paridad: Cheques

## Fecha
2026-03-31

## Referencia funcional
`C:\Zulu\zuluApp`

## Objetivo
Dejar trazado qué quedó cubierto en backend para `ventas/cheques` y qué sigue pendiente respecto del comportamiento legacy.

## Cubierto en backend

### Dominio `Cheque`
- soporte de `TipoCheque` (`Tercero` / `Propio`)
- flags `EsALaOrden` y `EsCruzado`
- datos bancarios extendidos: `CodigoSucursalBancaria`, `CodigoPostal`
- `Titular`
- referencias `ChequeraId` y `ComprobanteOrigenId`
- `ConceptoRechazo`
- operaciones de dominio:
  - `Depositar`
  - `Acreditar`
  - `Rechazar`
  - `Entregar`
  - `Endosar`
  - `Anular`
  - `ActualizarDatos`

### Contratos de aplicación
- `CreateChequeCommand` alineado con la firma real de `Cheque.Crear`
- validaciones de creación para:
  - tipo válido
  - titular obligatorio en cheque de terceros
  - chequera obligatoria en cheque propio
- `CambiarEstadoChequeCommand` extendido con:
  - `Endosar`
  - `Anular`
  - `ConceptoRechazo`
- validaciones de cambio de estado para:
  - fecha obligatoria en depósito y acreditación
  - tercero obligatorio en entrega y endoso
  - concepto obligatorio en rechazo
  - motivo obligatorio en anulación

### Lectura y filtros
- `ChequeDto` con campos extendidos ya visibles en backend
- `GetChequesPagedQuery` extendido con filtros por:
  - `Tipo`
  - `EsALaOrden`
  - `EsCruzado`
  - `Titular`
- `GetChequesPagedQueryHandler` enriquecido con:
  - `CajaDescripcion`
  - `TerceroRazonSocial`
  - `MonedaSimbolo`
  - `ChequeraDescripcion`
  - `ComprobanteOrigenNumero`

### API
- `ChequesController` normalizado para usar `CambiarEstadoChequeCommand`
- endpoints funcionales para:
  - `depositar`
  - `acreditar`
  - `rechazar`
  - `entregar`
  - `endosar`
  - `anular`

### Trazabilidad
- auditoría registrada para:
  - alta
  - depósito
  - acreditación
  - rechazo
  - entrega
  - endoso
  - anulación

## Pendiente para paridad total
- query específica de cheques propios
- query específica de cheques de terceros
- detalle/ruta completa equivalente a la vista legacy más rica
- emisión de cheque propio desde chequera
- catálogo formal de conceptos de rechazo
- tests adicionales de flujos de endoso y anulación
- validación manual contra PostgreSQL local `localhost:5432`

## Archivos impactados
- `src/ZuluIA_Back.Application/Features/Cheques/Commands/CreateChequeCommand.cs`
- `src/ZuluIA_Back.Application/Features/Cheques/Commands/CreateChequeCommandValidator.cs`
- `src/ZuluIA_Back.Application/Features/Cheques/Commands/CreateChequeCommandHandler.cs`
- `src/ZuluIA_Back.Application/Features/Cheques/Commands/CambiarEstadoChequeCommand.cs`
- `src/ZuluIA_Back.Application/Features/Cheques/Commands/CambiarEstadoChequeCommandValidator.cs`
- `src/ZuluIA_Back.Application/Features/Cheques/Commands/CambiarEstadoChequeCommandHandler.cs`
- `src/ZuluIA_Back.Application/Features/Cheques/Queries/GetChequesPagedQuery.cs`
- `src/ZuluIA_Back.Application/Features/Cheques/Queries/GetChequesPagedQueryHandler.cs`
- `src/ZuluIA_Back.Api/Controllers/ChequesController.cs`
- `src/ZuluIA_Back.Infrastructure/Persistence/Repositories/ChequeRepository.cs`
- `src/ZuluIA_Back.Domain/Interfaces/IChequeRepository.cs`
- `src/ZuluIA_Back.Domain/Enums/TipoOperacionCheque.cs`
- `src/ZuluIA_Back.Application/Common/Mappings/MappingProfile.cs`
- `tests/ZuluIA_Back.UnitTests/Domain/ChequeTests.cs`
- `tests/ZuluIA_Back.UnitTests/Application/Handlers/CajasChequeHandlerTests.cs`

## Base de datos
La documentación de upgrade sigue en:
- `docs/sql/2024-03-20-cheques-paridad-zuluapp-upgrade.md`

## Próximo corte recomendado
1. compilar
2. corregir errores de integración restantes
3. agregar tests unitarios de `Endosar` y `Anular`
4. completar query de cheques propios / terceros
5. validar contra PostgreSQL local
