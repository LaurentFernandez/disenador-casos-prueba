# Diseñador de casos de prueba

[![CI](https://github.com/LaurentFernandez/disenador-casos-prueba/actions/workflows/ci.yml/badge.svg)](https://github.com/LaurentFernandez/disenador-casos-prueba/actions/workflows/ci.yml)

Aplicación web que genera casos de prueba a partir de historias de usuario aplicando técnicas formales de diseño de pruebas (ISTQB): partición de equivalencia, valores límite, tablas de decisión y combinatoria por pares. Muestra cada caso con la técnica que lo originó, lo convierte a Gherkin y lo exporta a CSV, Markdown y `.feature`.

> **Estado:** en construcción (Fase 0). Ver el [plan](docs/plan.md) y las [historias de usuario](docs/historias-de-usuario.md).

*English:* a web app that generates test cases from user stories using formal black-box test design techniques (equivalence partitioning, boundary value analysis, decision tables and pairwise testing). Work in progress.

## Tecnologías

- .NET 10 y Blazor WebAssembly (aplicación instalable como PWA, sin servidor)
- xUnit para las pruebas
- GitHub Actions para compilar y probar en cada cambio

## Estructura

```
src/
  Casos.Core/        motor: las técnicas de diseño de pruebas
  Casos.Web/         interfaz web (Blazor WebAssembly, PWA)
tests/
  Casos.Core.Tests/  pruebas unitarias del motor
docs/                plan e historias de usuario
```

## Cómo ejecutarlo

Requiere el [SDK de .NET 10](https://dotnet.microsoft.com/download).

```bash
dotnet test                                 # ejecutar las pruebas
dotnet run --project src/Casos.Web          # abrir la aplicación en local
```

## Autor

[Laurent Andrés Fernandez](https://laurentfernandez.github.io/portafolio/)
