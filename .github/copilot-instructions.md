# Copilot Instructions

## Directrices del proyecto
- Al planificar módulos funcionales, usar `C:\Zulu\zuluApp` como referencia principal y priorizar que el backend cubra el 100% de la funcionalidad de las vistas de la sección de ventas, asegurando que todos los datos visibles y operativos que consume el frontend nuevo estén cubiertos, evitando faltantes antes de ajustar el front.
- Cerrar primero el backend de ventas al 100% de la funcionalidad de `C:\Zulu\zuluApp` antes de avanzar cualquier frontend nuevo.

## Documentación de Base de Datos
- Documentar cualquier contenido de base de datos, scripts SQL y queries operativas en archivos `.md`, no en `.sql`, porque los archivos `.sql` frenan pruebas y compilaciones al intentar ejecutarse. En este proyecto no deben existir archivos `.sql`; cualquier script o query operativa debe eliminarse o migrarse a archivos `.md` porque no hay base de datos integrada en este workspace y los `.sql` frenan compilaciones/pruebas.
- La validación de BD debe hacerse contra PostgreSQL local en `localhost:5432`.