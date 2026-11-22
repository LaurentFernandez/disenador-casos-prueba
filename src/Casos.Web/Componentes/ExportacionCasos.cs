using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Casos.Core.Modelo;
using Casos.Web.Recursos;

namespace Casos.Web.Componentes;

/// <summary>
/// Los casos generados como archivos para descargar: CSV para Excel, Jira o TestRail, Markdown para la
/// documentación y .feature para el proyecto de automatización.
/// </summary>
public static class ExportacionCasos
{
    // Excel en español espera punto y coma, y solo reconoce UTF-8 si el archivo empieza con esta marca.
    private const char SeparadorCsv = ';';
    private const string MarcaUtf8 = "﻿";
    private const string FinDeLineaCsv = "\r\n";

    // Una celda que empieza con uno de estos caracteres puede ejecutarse como fórmula en una hoja de cálculo.
    private const string InicioDeFormula = "=+-@\t\r";

    /// <summary>
    /// Un archivo listo para descargar.
    /// </summary>
    public sealed record Archivo(string Nombre, string Tipo, string Contenido);

    /// <summary>
    /// CSV con el valor completo de cada dato, para importarlo o ejecutar los casos.
    /// </summary>
    public static Archivo Csv(Historia historia, IReadOnlyList<CasoPrueba> casos)
    {
        var filas = casos.Select(caso => Celdas(historia, caso, valor => valor)).Prepend(Encabezados(historia));
        var contenido = string.Join(FinDeLineaCsv, filas.Select(fila => string.Join(SeparadorCsv, fila.Select(CeldaCsv))));
        return new Archivo($"{NombreArchivo(historia.Nombre)}.csv", "text/csv;charset=utf-8", MarcaUtf8 + contenido + FinDeLineaCsv);
    }

    /// <summary>
    /// Markdown con la tabla tal como se ve en pantalla, para la documentación.
    /// </summary>
    public static Archivo Markdown(Historia historia, IReadOnlyList<CasoPrueba> casos)
    {
        var encabezados = Encabezados(historia).ToList();
        var lineas = new List<string>
        {
            $"# {CeldaMarkdown(historia.Nombre)}",
            "",
            TextosCaso.Resumen(casos),
            "",
            FilaMarkdown(encabezados),
            FilaMarkdown(encabezados.Select(_ => "---")),
        };
        lineas.AddRange(casos.Select(caso => FilaMarkdown(Celdas(historia, caso, TextosCaso.Valor))));
        return new Archivo($"{NombreArchivo(historia.Nombre)}.md", "text/markdown;charset=utf-8", string.Join("\n", lineas) + "\n");
    }

    /// <summary>
    /// El .feature que muestra la vista Gherkin.
    /// </summary>
    public static Archivo Feature(Historia historia, IReadOnlyList<CasoPrueba> casos) =>
        new($"{NombreArchivo(historia.Nombre)}.feature", "text/plain;charset=utf-8", GherkinCasos.Caracteristica(historia, casos));

    /// <summary>
    /// El nombre de la historia en minúsculas, sin tildes y con guiones: «Recuperación de contraseña» da «recuperacion-de-contrasena».
    /// </summary>
    public static string NombreArchivo(string historia)
    {
        var sinTildes = string.Concat(historia.Normalize(NormalizationForm.FormD)
            .Where(caracter => CharUnicodeInfo.GetUnicodeCategory(caracter) != UnicodeCategory.NonSpacingMark));
        var palabras = Regex.Split(sinTildes.ToLowerInvariant(), "[^a-z0-9]+").Where(palabra => palabra.Length > 0);
        var nombre = string.Join('-', palabras);
        return nombre.Length > 0 ? nombre : Textos.De("Exportar.NombreSinLetras");
    }

    private static IEnumerable<string> Encabezados(Historia historia) =>
    [
        Textos.De("Columna.Codigo"),
        Textos.De("Columna.Descripcion"),
        Textos.De("Columna.Tecnica"),
        .. historia.Campos.Select(campo => campo.Nombre),
        Textos.De("Columna.Resultado"),
    ];

    private static IEnumerable<string> Celdas(Historia historia, CasoPrueba caso, Func<string, string> valorVisible) =>
    [
        caso.Codigo,
        TextosCaso.Descripcion(caso),
        TextosCaso.NombreLargoTecnica(caso.Tecnica),
        .. historia.Campos.Select(campo => valorVisible(caso.Datos[campo.Nombre])),
        TextosCaso.Resultado(caso),
    ];

    private static string CeldaCsv(string valor)
    {
        // Lo que parece fórmula se marca como texto; un número negativo se deja como está.
        if (valor.Length > 0 && InicioDeFormula.Contains(valor[0])
            && !double.TryParse(valor, NumberStyles.Float, CultureInfo.InvariantCulture, out _))
        {
            valor = "'" + valor;
        }

        return valor.IndexOfAny([SeparadorCsv, '"', '\n', '\r']) >= 0 ? $"\"{valor.Replace("\"", "\"\"")}\"" : valor;
    }

    private static string FilaMarkdown(IEnumerable<string> celdas) => $"| {string.Join(" | ", celdas.Select(CeldaMarkdown))} |";

    // La barra cortaría la columna y el «<» podría abrir una etiqueta HTML al mostrar el archivo.
    private static string CeldaMarkdown(string valor) => valor.Replace("|", "\\|").Replace("<", "&lt;");
}
