using Microsoft.Playwright;

namespace Casos.E2E.Tests;

[Collection(ColeccionAplicacion.Nombre)]
public class ConservarHistoriaTests(AplicacionEnMarcha aplicacion) : PruebaEnNavegador(aplicacion)
{
    [Fact]
    public async Task Recargar_ConservaLaHistoriaConSusCamposYSuReglaComoEnHU15()
    {
        await AbrirAsync();
        await CargarCarritoConReglaAsync();

        await RecargarAsync();

        await Expect(Page.Locator("#nombre-historia")).ToHaveValueAsync("Carrito de compras");
        await Expect(Etiqueta("Campo obligatorio de la regla 1")).ToHaveValueAsync("cupón");
        await Expect(Resumen).ToHaveTextAsync(ResumenCarritoConRegla);
    }

    [Fact]
    public async Task CambiarIdioma_ConservaLosCamposComoEnHU15()
    {
        await AbrirAsync();
        await EscribirEdadAsync();

        await Boton("English (EN)").ClickAsync();

        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Story fields", Level = 1 })).ToBeVisibleAsync(new() { Timeout = EsperaDeCarga });
        await Expect(Etiqueta("Field 1 name")).ToHaveValueAsync("edad");
        await Expect(Resumen).ToHaveTextAsync("12 cases: 5 accepted and 7 rejected.");
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
}
