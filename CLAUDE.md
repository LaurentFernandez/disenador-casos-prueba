# Diseñador de casos de prueba: contexto del proyecto

## Qué es

Aplicación web (Blazor WebAssembly, instalable como PWA) que genera casos de prueba a partir de historias de usuario aplicando técnicas de diseño de pruebas de caja negra (ISTQB): partición de equivalencia, valores límite, tablas de decisión y combinatoria por pares (pairwise). Muestra los casos en una tabla con su técnica de origen, los convierte a Gherkin y los exporta a CSV, Markdown y `.feature`.

Es un proyecto de portafolio de Laurent Andrés Fernandez (ingeniero de software en NyGsoft, con experiencia en desarrollo .NET y QA). Su objetivo es mostrar análisis, QA y desarrollo .NET en un solo proyecto, con demo en vivo. El plan completo está en `docs/plan.md`.

## Decisiones tomadas (no reabrir sin preguntar a Laurent)

- **Costo cero:** solo herramientas gratuitas u open source. Nada de APIs de pago. Sin IA en el núcleo: el modo IA local (Fase 4, HU-14) solo propone filas de la hoja con Ollama, en el equipo del usuario, y aparece solo cuando la aplicación corre en `localhost`.
- **Aplicación web** en Blazor WebAssembly con PWA, publicada gratis en GitHub Pages, sin servidor: `https://laurentfernandez.github.io/disenador-casos-prueba/`. La publican dos trabajos de `ci.yml` en cada push a `main` con las pruebas en verde: `publicar` compila y sube el paquete, y `desplegar` lo pone en GitHub Pages. La base es relativa (`<base href="./" />`) para que la misma compilación sirva en local y en la subcarpeta; no se modifica el `index.html` publicado porque rompería la verificación de integridad del service worker. El despliegue va en un trabajo aparte porque, en el mismo trabajo que sube el paquete, la API de GitHub a veces todavía no lo veía ("No artifacts named github-pages"). Si `desplegar` falla, basta con volver a ejecutar solo los trabajos fallidos (`gh run rerun <id> --failed`), que no suben otro paquete. Si alguna vez una ejecución queda con dos paquetes "github-pages" ("Multiple artifacts"), hay que borrar los sobrantes (`gh api -X DELETE repos/LaurentFernandez/disenador-casos-prueba/actions/artifacts/<id>`) antes de reintentar.
- **.NET 10 (LTS).** Pruebas: xUnit para el motor y para la lógica de la interfaz, bUnit para los componentes y Playwright para .NET para las de extremo a extremo (en Chromium). Las de extremo a extremo levantan la aplicación con `dotnet run --no-build` en un puerto libre (`AplicacionEnMarcha`), una sola vez para todas, y la abren en español. El CI instala Chromium antes de `dotnet test`, así que también deciden si se publica.
- **Interfaz en español e inglés**, con `IStringLocalizer` y archivos `.resx`: los textos están en `Recursos/Textos.resx` (español) y `Recursos/Textos.en.resx` (inglés), y se leen con la clase estática `Textos`. El idioma se fija al arrancar (el elegido antes o, la primera vez, inglés si el navegador está en inglés y español en cualquier otro caso); cambiarlo guarda la elección y recarga la página. Los nombres que escribe el usuario no se traducen y el CSV usa ";" en los dos idiomas. Las pruebas de textos fijan su idioma heredando de `PruebaEnIdioma`.
- **Código en español:** clases, métodos, variables y nombres de pruebas, igual que los demás proyectos de Laurent.
- **El motor (`Casos.Core`) no depende de la interfaz.** Toda la lógica vive y se prueba ahí; `Casos.Web` solo presenta.
- **El motor no devuelve textos para mostrar:** devuelve tipos (por ejemplo, los enums `TipoClase` y `ErrorCampo`) y la interfaz arma la descripción o el aviso en español o inglés. Lo mismo pasa con el valor de una opción fuera de la lista: el motor marca el caso con `TipoClase.FueraDeLista`, y la interfaz lo muestra en su idioma ("Opción no válida" o "Invalid option") con `TextosCaso.Dato`, que usan todas las vistas y las exportaciones. `CampoLista.ValorQueNoEsOpcion` evita que ese valor coincida con una de las opciones: si coincide, le agrega un número.
- **Textos neutros:** los nombres de los campos los escribe el usuario, así que los textos de los casos evitan el género gramatical ("Edad sin valor", "Rechaza el valor de edad") y un texto largo se describe por su longitud ("un texto de 26 caracteres").
- **La historia se conserva en el navegador (HU-15):** la página guarda en `localStorage` (clave `historia`, con `wwwroot/js/almacen.js`) el nombre, las filas de campos y de reglas, la casilla de pares y si hubo cambios propios, después de cada cambio, y lo restaura al abrir. Se guarda una sola historia, sin botón "Guardar", y una hoja vacía borra lo guardado. `HistoriaGuardada` lo pasa a JSON y lo lee de vuelta: lo que no se entiende se descarta y los textos que falten se completan, para que un dato dañado no rompa la página. "Empezar de nuevo" vacía la hoja con la misma confirmación que los ejemplos.
- **Casos en vivo:** los casos se generan cada vez que cambia la hoja de campos, sin botón. Si alguna fila tiene un aviso, no se muestran casos hasta corregirla. La longitud máxima de un texto es 10 000 caracteres (`CampoTexto.LongitudMaximaPermitida`).
- **Repositorio:** `github.com/LaurentFernandez/disenador-casos-prueba`, rama `main`. Público.
- **Diseño visual:** está en `docs/diseno/maqueta.html`. Los campos se editan en una hoja tipo cuadrícula. Los casos se ven en tres vistas a elección del usuario: Matriz (una columna por campo, con el valor probado resaltado), Recta (el diagrama de partición de cada campo) y Gherkin. El estilo es el mismo en todas: títulos en Barlow (incluida en el proyecto, licencia OFL), azul tinta, lo válido en azul verdoso y lo inválido rayado en naranja. Colores, tamaños y espaciados son variables CSS en `wwwroot/css/app.css`; no se usa Bootstrap. El ícono de la aplicación es la recta de partición sobre azul tinta: el original es `docs/diseno/icono.svg` y de él salen `favicon.png`, `icon-192.png` e `icon-512.png` (este también como "maskable"). El manifiesto queda en español porque es un archivo fijo.

## Estructura

```
DisenadorCasosPrueba.slnx
src/Casos.Core/          motor (modelo en Modelo/, técnicas en Tecnicas/)
src/Casos.Web/           interfaz Blazor WebAssembly + PWA (componentes en Componentes/, textos en Recursos/)
tests/Casos.Core.Tests/  pruebas xUnit del motor
tests/Casos.Web.Tests/   pruebas xUnit de la lógica de la interfaz y bUnit de sus componentes
tests/Casos.E2E.Tests/   pruebas de extremo a extremo con Playwright
docs/plan.md             objetivo, técnicas, arquitectura y fases
docs/historias-de-usuario.md   requerimientos (los escribe Laurent)
docs/diseno/maqueta.html       maqueta de la interfaz (se abre en el navegador)
.github/workflows/ci.yml compila y prueba en cada push a main y en cada PR; publica en GitHub Pages lo que llega a main
```

## Comandos

```bash
dotnet build
dotnet test
dotnet run --project src/Casos.Web
```

Las pruebas de extremo a extremo necesitan el Chromium de Playwright, que se instala una vez después de compilar. En esta máquina no hay PowerShell 7, así que se usa el instalador que trae el paquete:

```bash
tests/Casos.E2E.Tests/bin/Debug/net10.0/.playwright/node/win32_x64/node.exe tests/Casos.E2E.Tests/bin/Debug/net10.0/.playwright/package/cli.js install chromium
```

El modo IA local necesita Ollama en marcha con el modelo de `wwwroot/appsettings.json`. En esta máquina Ollama está instalado (con `winget`, en `%LOCALAPPDATA%\Programs\Ollama`) y tiene `llama3.2`. Ninguna prueba lo necesita.

## Cómo trabajamos

- **Laurent es el analista.** Escribe las historias de usuario en `docs/historias-de-usuario.md`, y cada funcionalidad nace de una historia. Si una tarea no tiene historia, pídesela o propón un borrador marcado como tal para que él lo revise.
- **Laurent programa las partes que quiera aprender.** Claude prepara la base, explica las decisiones y revisa su código.
- Un issue por historia, una rama por funcionalidad y pull requests hacia `main`.
- Las reglas globales de Laurent (`~/.claude/CLAUDE.md`) aplican aquí: respuestas en español, confirmación explícita antes de cada commit, push o publicación, código limpio y verificación antes de decir "listo".

## Estado actual

- **Fases 0, 1 y 2 terminadas.** El motor tiene el modelo de campos con sus validaciones (`Modelo/`), la partición de equivalencia (`Tecnicas/ParticionEquivalencia`) y los valores límite de enteros y de longitud de texto (`Tecnicas/ValoresLimite`).
- **Interfaz (Fase 2).** Están la generación de casos en el motor (`GeneradorCasos`), el estilo base de la maqueta, la hoja de campos (`Componentes/HojaCampos.razor`), el nombre de la historia y las vistas Matriz, Recta y Gherkin (`Componentes/CasosGenerados.razor`; el Gherkin lo arma `GherkinCasos`), la exportación a CSV, Markdown y `.feature` (`ExportacionCasos` y `wwwroot/js/descargas.js`), el cambio de idioma (`Componentes/SelectorIdioma.razor` y `wwwroot/js/idioma.js`) y los ejemplos listos (`Componentes/EjemplosHistoria.cs`; la página pide confirmación con `confirm()` solo si se escribió algo desde la última carga). La demo está publicada en GitHub Pages, se puede instalar y funciona sin conexión gracias al service worker.
- **Fase 3 terminada.** Las dos técnicas avanzadas están hechas, en el motor y en la interfaz:
  - **Tabla de decisión (HU-11):** una regla dice "si un campo de lista tiene una opción, otro campo opcional pasa a ser obligatorio" (`Modelo/ReglaObligatorio`, validada por `Historia.AgregarRegla` con el enum `ErrorRegla`). `Tecnicas/TablaDecision` da 4 casos por regla, con una opción representativa para el "no". En la interfaz, las reglas se arman en `Componentes/HojaReglas.razor`.
  - **Combinación por pares (HU-12):** `Tecnicas/Pares` implementa IPO sobre los campos de lista y es determinista. Se activa con `Historia.CombinarPorPares`, que necesita al menos dos campos de lista (`PuedeCombinarPorPares`). En la interfaz, la activa la casilla de la sección "Combinación por pares" de `Pages/Home.razor`; con menos de dos listas avisa y oculta los casos, y si no, dice cuántas combinaciones salieron de las posibles. Cada caso por pares trae los campos que combina en `CasoPrueba.CamposCombinados`, que la Matriz resalta. El ejemplo "Compatibilidad" (navegador, sistema e idioma) es el único con `PorPares` en `EjemplosHistoria` y deja la casilla marcada al cargarlo; los demás la desmarcan.
  - Los casos de las dos técnicas van en las tres vistas (en la Recta solo en la tabla de casos) y en las exportaciones.
  - **Pruebas de componentes y de extremo a extremo (HU-13):** bUnit comprueba el resaltado de la Matriz, la leyenda y que una regla conserve el campo elegido al renombrarlo (`VistaMatrizTests`, `CasosGeneradosTests`, `HojaReglasTests`). Playwright recorre cargar un ejemplo, corregir un aviso, exportar el CSV y cambiar el idioma (`FlujosCompletosTests`).
- **Fase 4 terminada (opcional).** El modo IA local (HU-14) propone los campos a partir del texto de una historia:
  - `Componentes/ClienteOllama` pide a Ollama los campos con un esquema JSON fijo y temperatura 0, y convierte la respuesta en filas. La dirección y el modelo (`llama3.2`) están en `wwwroot/appsettings.json`. Una respuesta que no se entiende, Ollama apagado o el modelo sin bajar dan un aviso, no un error.
  - Las instrucciones del modelo están probadas con `llama3.2` en historias en español y en inglés, y llevan el idioma de la interfaz para que no traduzca la historia. Si se cambian, hay que volver a probarlas con historias reales: un ejemplo con números hizo que el modelo copiara esos números.
  - `Componentes/ProponerCampos.razor` es una sección plegable que solo aparece en `localhost` (Ollama además rechaza otros orígenes). Muestra la propuesta con los avisos de la hoja, y aceptarla reemplaza los campos, con confirmación si había campos propios.
  - Las pruebas usan `OllamaSimulado`; ninguna necesita Ollama.
- **Mejoras después del plan.** HU-15: la historia se conserva al recargar, cambiar de idioma o abrir sin conexión (ver "Decisiones tomadas").
- **Pruebas:** 69 del motor, 99 de la interfaz (88 de lógica y 11 de componentes) y 7 de extremo a extremo.
- **Historias:** HU-01 a HU-15 están hechas. HU-01 no tuvo issue porque se hizo antes de crear el repositorio. Son borradores de Claude que Laurent tiene que revisar los escenarios de HU-13, HU-14 y HU-15, los nuevos de HU-09 (ejemplo Compatibilidad) y el nuevo de HU-08 (valor fuera de la lista en inglés).
- **Siguiente:** las cinco fases del plan están hechas. Lo que siga sale de historias nuevas de Laurent; quedaron propuestas exportar e importar la definición de una historia, y los campos de fecha.

## Fases

| Fase | Contenido |
|---|---|
| 0. Arranque | Esqueleto, CI y primeras historias |
| 1. Motor | Modelo de campos, partición de equivalencia y valores límite |
| 2. Interfaz | Formulario, tabla de resultados, Gherkin, exportaciones, español e inglés, GitHub Pages |
| 3. Técnicas avanzadas | Tablas de decisión y pairwise; pruebas bUnit y Playwright |
| 4. Opcional | Modo IA local con Ollama |
