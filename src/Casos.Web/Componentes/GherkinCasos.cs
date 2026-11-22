using Casos.Core.Modelo;

namespace Casos.Web.Componentes;

// shortcut: palabras clave y pasos solo en español; agregar los de inglés con el cambio de idioma (HU-08).
/// <summary>
/// Los casos como escenarios Gherkin. Los pasos evitan el género gramatical porque los nombres de los campos
/// los escribe el usuario, y un texto se describe por su longitud para que el escenario se pueda leer.
/// </summary>
public static class GherkinCasos
{
    private const string Sangria = "  ";

    public static string Caracteristica(Historia historia, IReadOnlyList<CasoPrueba> casos)
    {
        var escenarios = string.Join("\n\n", casos.Select(caso => Escenario(historia, caso)));
        return $"# language: es\nCaracterística: {historia.Nombre}\n\n{escenarios}\n";
    }

    public static string Escenario(Historia historia, CasoPrueba caso)
    {
        var lineas = new List<string>
        {
            $"{Sangria}Escenario: {caso.Codigo} {TextosCaso.Descripcion(caso)}",
            $"{Sangria}{Sangria}Dado el formulario \"{historia.Nombre}\"",
        };
        for (var indice = 0; indice < historia.Campos.Count; indice++)
        {
            var campo = historia.Campos[indice];
            lineas.Add($"{Sangria}{Sangria}{(indice == 0 ? "Cuando" : "Y")} {Paso(campo, caso.Datos[campo.Nombre])}");
        }

        lineas.Add($"{Sangria}{Sangria}Entonces el sistema {ConMinuscula(TextosCaso.Resultado(caso))}");
        return string.Join("\n", lineas);
    }

    private static string Paso(Campo campo, string valor) => (campo, valor) switch
    {
        (_, "") => $"dejo {campo.Nombre} sin valor",
        (CampoTexto, _) => $"ingreso un texto de {TextosCaso.Caracteres(valor.Length)} en {campo.Nombre}",
        (CampoLista, _) => $"elijo \"{valor}\" en {campo.Nombre}",
        _ => $"ingreso \"{valor}\" en {campo.Nombre}",
    };

    private static string ConMinuscula(string texto) => string.Concat(texto[..1].ToLowerInvariant(), texto[1..]);
}
