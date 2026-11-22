using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Casos.Web.Recursos;

/// <summary>
/// Los textos de la interfaz en el idioma de la aplicación: Textos.resx en español y Textos.en.resx en inglés.
/// Es estático porque también lo usan las clases que arman los textos de los casos, que no reciben servicios.
/// </summary>
public static class Textos
{
    private static readonly IStringLocalizer Localizador =
        new ResourceManagerStringLocalizerFactory(Options.Create(new LocalizationOptions()), NullLoggerFactory.Instance)
            .Create(typeof(Textos));

    public static string De(string clave, params object[] argumentos) => Localizador[clave, argumentos];
}
