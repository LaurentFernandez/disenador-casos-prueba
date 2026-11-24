using Microsoft.Playwright;

namespace Casos.E2E.Tests;

[Collection(ColeccionAplicacion.Nombre)]
public class ConservarHistoriaTests(AplicacionEnMarcha aplicacion) : PruebaEnNavegador(aplicacion)
{
    private const string EdadDe18A65 = "12 casos: 5 se aceptan y 7 se rechazan.";

    [Fact]
    public async Task Recargar_ConservaLaHistoriaConSusCamposYSuReglaComoEnHU15()
    {
        await AbrirAsync();
        await Boton("Carrito de compras").ClickAsync();
        await Boton("Agregar regla").ClickAsync();
        await Etiqueta("Campo de la condición de la regla 1").SelectOptionAsync("medio de pago");
        await Etiqueta("Campo obligatorio de la regla 1").SelectOptionAsync("cupón");
        // Los 25 casos del carrito y los 4 de la tabla de decisión de la regla.
        await Expect(Page.Locator(".resumen")).ToHaveTextAsync("29 casos: 15 se aceptan y 14 se rechazan.");

        await RecargarAsync();

        await Expect(Page.Locator("#nombre-historia")).ToHaveValueAsync("Carrito de compras");
        await Expect(Etiqueta("Campo obligatorio de la regla 1")).ToHaveValueAsync("cupón");
        await Expect(Page.Locator(".resumen")).ToHaveTextAsync("29 casos: 15 se aceptan y 14 se rechazan.");
    }

    [Fact]
    public async Task CambiarIdioma_ConservaLosCamposComoEnHU15()
    {
        await AbrirAsync();
        await EscribirEdadAsync();

        await Boton("English (EN)").ClickAsync();

        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Story fields", Level = 1 })).ToBeVisibleAsync(new() { Timeout = EsperaDeCarga });
        await Expect(Etiqueta("Field 1 name")).ToHaveValueAsync("edad");
        await Expect(Page.Locator(".resumen")).ToHaveTextAsync("12 cases: 5 accepted and 7 rejected.");
    }

    [Fact]
    public async Task EmpezarDeNuevo_VaciaLaHojaYBorraLoGuardadoComoEnHU15()
    {
        var preguntas = 0;
        Page.Dialog += async (_, dialogo) =>
        {
            preguntas++;
            await dialogo.AcceptAsync();
        };
        await AbrirAsync();
        await EscribirEdadAsync();

        await Boton("Empezar de nuevo").ClickAsync();

        await Expect(Etiqueta("Nombre del campo 1")).ToHaveValueAsync("");
        await Expect(Page.GetByText("Agrega un campo a la hoja para ver sus casos.")).ToBeVisibleAsync();
        Assert.Equal(1, preguntas);
        // Al recargar, la hoja vacía se ve antes de restaurar; lo que prueba que se borró es el almacenamiento.
        await Page.WaitForFunctionAsync("() => localStorage.getItem('historia') === null");
        await RecargarAsync();
        await Expect(Etiqueta("Nombre del campo 1")).ToHaveValueAsync("");
    }

    private ILocator Etiqueta(string texto) => Page.GetByLabel(texto, new() { Exact = true });

    private async Task EscribirEdadAsync()
    {
        await EscribirAsync("Nombre del campo 1", "edad");
        await EscribirAsync("Mínimo del campo 1", "18");
        await EscribirAsync("Máximo del campo 1", "65");
        await Expect(Page.Locator(".resumen")).ToHaveTextAsync(EdadDe18A65);
    }
}
