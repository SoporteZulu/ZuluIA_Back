# Copilot Instructions

## Directrices del proyecto
- Al planificar módulos funcionales, usar `C:\Zulu\ZuluIA_Front` como referencia principal para construir CRM, ya que `C:\Zulu\zuluApp` no es una referencia funcional para ese módulo. Priorizar que el backend cubra el 100% de la funcionalidad de las vistas de la sección de ventas, asegurando que todos los datos visibles y operativos que consume el frontend nuevo estén cubiertos, evitando faltantes antes de ajustar el front. El usuario prioriza la paridad funcional lo más cercana al 100% con `zuluApp` y obtener resultados lo antes posible, prefiriendo la vía más rápida que mantenga el mismo comportamiento visible y persistencia funcional.
- Cerrar primero el backend de ventas al 100% de la funcionalidad de `C:\Zulu\zuluApp` antes de avanzar cualquier frontend nuevo.
- CRM es una sección paralela a Ventas. Ventas solo se usa como referencia funcional para crear servicios de CRM; no deben tratarse como la misma sección ni describirse CRM como parte de Ventas.
- Para CRM, `C:\Zulu\ZuluIA_Front` es solo una base V0. El backend debe definir las reglas del módulo CRM, cubrir vista por vista y formulario por formulario, y no limitarse estrictamente a lo ya expuesto por el frontend. La base de datos se diseñará después en función de estas definiciones backend.

## Documentación de Base de Datos
- Documentar cualquier contenido de base de datos, scripts SQL y queries operativas en archivos `.md`, no en `.sql`, porque los archivos `.sql` frenan pruebas y compilaciones al intentar ejecutarse. En este proyecto no deben existir archivos `.sql`; cualquier script o query operativa debe eliminarse o migrarse a archivos `.md` porque no hay base de datos integrada en este workspace y los `.sql` frenan compilaciones/pruebas.
- La validación de BD debe hacerse contra PostgreSQL local en `localhost:5432`.
- Cuando se genere un archivo `.md` con script SQL para este proyecto, no debe incluir fences Markdown como ```sql o ``` porque el usuario copia y pega el contenido completo directamente en la base de datos.