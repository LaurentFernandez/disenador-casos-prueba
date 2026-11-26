using System.Globalization;
using Casos.Core.Modelo;

namespace Casos.Core.Tecnicas;

/// <summary>
/// Cómo se escriben los valores de ejemplo, para que todas las técnicas los escriban igual.
/// </summary>
internal static class Ejemplos
{
    private const char CaracterDeRelleno = 'a';

    // Cultura invariante: los números usan punto decimal sin importar el idioma del equipo.
    public static string Numero(long valor) => valor.ToString(CultureInfo.InvariantCulture);

    public static string Numero(decimal valor) => valor.ToString(CultureInfo.InvariantCulture);

    public static string TextoDe(long longitud) => new(CaracterDeRelleno, (int)longitud);

    public static string Fecha(DateOnly fecha) => fecha.ToString(CampoFecha.Formato, CultureInfo.InvariantCulture);
}
