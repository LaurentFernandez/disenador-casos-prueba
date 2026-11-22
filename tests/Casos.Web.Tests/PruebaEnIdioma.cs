using System.Globalization;

namespace Casos.Web.Tests;

/// <summary>
/// Fija el idioma de cada prueba, para que los textos esperados no dependan del idioma de la máquina que la ejecuta.
/// </summary>
public abstract class PruebaEnIdioma
{
    protected PruebaEnIdioma(string idioma)
    {
        CultureInfo.CurrentCulture = CultureInfo.CurrentUICulture = new CultureInfo(idioma);
    }
}
