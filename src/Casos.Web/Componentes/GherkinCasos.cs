using Casos.Core.Modelo;
using Casos.Web.Recursos;

namespace Casos.Web.Componentes;

/// <summary>
/// Los casos como escenarios Gherkin, con las palabras clave del idioma de la aplicación. Los pasos evitan el
/// género gramatical porque los nombres de los campos los escribe el usuario, y un texto se describe por su
/// longitud para que el escenario se pueda leer.
/// </summary>
public static class GherkinCasos
{
    private const string Sangria = "  ";

    public static string Caracteristica(Historia historia, IReadOnlyList<CasoPrueba> casos)
    {
        var escenarios = string.Join("\n\n", casos.Select(caso => Escenario(historia, caso)));
        return $"# language: {Textos.De("Gherkin.Idioma")}\n{Textos.De("Gherkin.Caracteristica")}: {historia.Nombre}\n\n{escenarios}\n";
    }

    public static string Escenario(Historia historia, CasoPrueba caso)
    {
        var lineas = new List<string>
        {
            $"{Sangria}{Textos.De("Gherkin.Escenario")}: {caso.Codigo} {TextosCaso.Descripcion(caso)}",
            Paso("Gherkin.Dado", Textos.De("Gherkin.Formulario", historia.Nombre)),
        };
        for (var indice = 0; indice < historia.Campos.Count; indice++)
        {
            var campo = historia.Campos[indice];
            lineas.Add(Paso(indice == 0 ? "Gherkin.Cuando" : "Gherkin.Y", Accion(campo, TextosCaso.Dato(caso, campo))));
        }

        lineas.Add(Paso("Gherkin.Entonces", Textos.De("Gherkin.Sistema", ConMinuscula(TextosCaso.Resultado(caso)))));
        return string.Join("\n", lineas);
    }

    private static string Paso(string palabraClave, string texto) => $"{Sangria}{Sangria}{Textos.De(palabraClave)} {texto}";

    private static string Accion(Campo campo, string valor) => (campo, valor) switch
    {
        (_, "") => Textos.De("Gherkin.Dejo", campo.Nombre),
        (CampoTexto, _) => Textos.De("Gherkin.IngresoTexto", TextosCaso.Caracteres(valor.Length), campo.Nombre),
        (CampoLista, _) => Textos.De("Gherkin.Elijo", valor, campo.Nombre),
        _ => Textos.De("Gherkin.Ingreso", valor, campo.Nombre),
    };

    private static string ConMinuscula(string texto) => string.Concat(texto[..1].ToLowerInvariant(), texto[1..]);
}
