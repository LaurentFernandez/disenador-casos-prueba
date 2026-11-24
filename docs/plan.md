# Plan del proyecto

## Objetivo

Una aplicación web que genere casos de prueba a partir de historias de usuario aplicando técnicas formales de diseño de pruebas de caja negra (ISTQB). Es un proyecto de portafolio: debe mostrar análisis, QA y desarrollo .NET, con demo en vivo y costo cero.

## Cómo se usa

1. **Historia y campos:** el usuario escribe la historia y define los campos (nombre, tipo y reglas; por ejemplo, "edad: entero entre 18 y 65, obligatorio").
2. **Reglas de negocio** (opcional): condiciones como "si es Empresa, el NIT es obligatorio".
3. **Resultados:** tabla de casos de prueba con código, descripción, técnica de origen, datos y resultado esperado, más cada caso en Gherkin.
4. **Exportar:** CSV (Excel, Jira, TestRail), Markdown y `.feature`.

## Técnicas

| Técnica | Qué genera |
|---|---|
| Partición de equivalencia | Una prueba por clase válida e inválida de cada campo |
| Análisis de valores límite (tres valores) | El borde y sus dos vecinos en cada extremo del rango |
| Tablas de decisión | Una prueba por combinación relevante de condiciones de negocio |
| Combinatoria por pares (pairwise) | El mínimo de pruebas que cubre todas las parejas de valores entre campos |

## Arquitectura

- `Casos.Core`: el motor, en C# puro y sin dependencias de interfaz. Toda la lógica vive y se prueba aquí.
- `Casos.Web`: interfaz en Blazor WebAssembly, instalable como PWA. Se ejecuta completa en el navegador, sin servidor.
- `Casos.Core.Tests`: pruebas unitarias del motor con xUnit.
- `Casos.Web.Tests`: pruebas de la lógica de la interfaz con xUnit y de sus componentes con bUnit.
- `Casos.E2E.Tests`: pruebas de extremo a extremo con Playwright para .NET, que abren la aplicación en Chromium.
- GitHub Actions compila y prueba en cada push; desde la Fase 2 también publica en GitHub Pages.

## Herramientas (todas gratuitas)

.NET 10 (LTS), Blazor WebAssembly, xUnit, bUnit, Playwright para .NET, GitHub Actions y GitHub Pages. Para el modo IA local opcional, Ollama con un modelo abierto (`llama3.2`), que corre en el equipo del usuario.

## Fases

| Fase | Contenido | Estado |
|---|---|---|
| 0. Arranque | Esqueleto de la solución, CI, primeras historias de usuario | Hecha |
| 1. Motor | Modelo de campos, partición de equivalencia y valores límite, con pruebas | Hecha |
| 2. Interfaz | Formulario de campos, tabla de resultados, Gherkin, exportaciones, español e inglés, publicación en GitHub Pages | Hecha |
| 3. Técnicas avanzadas | Tablas de decisión y pairwise; pruebas bUnit y Playwright | Hecha |
| 4. Opcional | Modo IA local con Ollama para proponer campos a partir de texto libre | Hecha |
