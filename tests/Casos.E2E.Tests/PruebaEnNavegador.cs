using Microsoft.Playwright;
using Microsoft.Playwright.Xunit;

namespace Casos.E2E.Tests;

/// <summary>
/// Una página de Chromium nueva para cada prueba, con la aplicación en español.
/// </summary>
public abstract class PruebaEnNavegador(AplicacionEnMarcha aplicacion) : PageTest
{
    // La primera carga baja el runtime de .NET al navegador y tarda más que la espera normal de Playwright.
    protected const float EsperaDeCarga = 60_000;

    // Sin un idioma guardado, la aplicación usa el del navegador.
    public override BrowserNewContextOptions ContextOptions() => new() { Locale = "es-CO" };

    protected async Task AbrirAsync()
    {
        await Page.GotoAsync(aplicacion.Direccion);
        await Expect(Page.Locator("table.hoja")).ToBeVisibleAsync(new() { Timeout = EsperaDeCarga });
    }

    protected ILocator Boton(string nombre) => Page.GetByRole(AriaRole.Button, new() { Name = nombre, Exact = true });

    // La hoja guarda cada valor al salir del control, como pasa al usar el teclado o el mouse.
    protected async Task EscribirAsync(string etiqueta, string texto)
    {
        var control = Page.GetByLabel(etiqueta, new() { Exact = true });
        await control.FillAsync(texto);
        await control.BlurAsync();
    }
}
