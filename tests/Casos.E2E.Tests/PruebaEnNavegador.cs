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
        await EsperarCargaAsync();
    }

    protected async Task RecargarAsync()
    {
        await Page.ReloadAsync();
        await EsperarCargaAsync();
    }

    private Task EsperarCargaAsync() => Expect(Page.Locator("table.hoja")).ToBeVisibleAsync(new() { Timeout = EsperaDeCarga });

    protected const string ResumenEdadDe18A65 = "12 casos: 5 se aceptan y 7 se rechazan.";

    // Los 25 casos del carrito y los 4 de la tabla de decisión de su regla.
    protected const string ResumenCarritoConRegla = "29 casos: 15 se aceptan y 14 se rechazan.";

    protected ILocator Boton(string nombre) => Page.GetByRole(AriaRole.Button, new() { Name = nombre, Exact = true });

    protected ILocator Etiqueta(string texto) => Page.GetByLabel(texto, new() { Exact = true });

    protected ILocator Resumen => Page.Locator(".resumen");

    protected async Task EscribirEdadAsync()
    {
        await EscribirAsync("Nombre del campo 1", "edad");
        await EscribirAsync("Mínimo del campo 1", "18");
        await EscribirAsync("Máximo del campo 1", "65");
        await Expect(Resumen).ToHaveTextAsync(ResumenEdadDe18A65);
    }

    // El ejemplo del carrito más la regla «si el medio de pago es Tarjeta, el cupón es obligatorio».
    protected async Task CargarCarritoConReglaAsync()
    {
        await Boton("Carrito de compras").ClickAsync();
        await Boton("Agregar regla").ClickAsync();
        await Etiqueta("Campo de la condición de la regla 1").SelectOptionAsync("medio de pago");
        await Etiqueta("Campo obligatorio de la regla 1").SelectOptionAsync("cupón");
        await Expect(Resumen).ToHaveTextAsync(ResumenCarritoConRegla);
    }

    // La hoja guarda cada valor al salir del control, como pasa al usar el teclado o el mouse.
    protected async Task EscribirAsync(string etiqueta, string texto)
    {
        var control = Page.GetByLabel(etiqueta, new() { Exact = true });
        await control.FillAsync(texto);
        await control.BlurAsync();
    }
}
