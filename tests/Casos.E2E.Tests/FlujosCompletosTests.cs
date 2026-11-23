using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Playwright;

namespace Casos.E2E.Tests;

[Collection(ColeccionAplicacion.Nombre)]
public class FlujosCompletosTests(AplicacionEnMarcha aplicacion) : PruebaEnNavegador(aplicacion)
{
    private const string AvisoDeRango = "Escribe el mínimo y el máximo.";

    [Fact]
    public async Task CargarUnEjemplo_MuestraSusCasosEnLaMatrizYEnGherkin()
    {
        await AbrirAsync();

        await Boton("Registro de usuario").ClickAsync();

        await Expect(Page.Locator(".resumen")).ToHaveTextAsync("24 casos: 10 se aceptan y 14 se rechazan.");
        await Expect(Page.Locator("table.matriz tbody tr")).ToHaveCountAsync(24);
        await Page.GetByRole(AriaRole.Tab, new() { Name = "Gherkin" }).ClickAsync();
        Assert.Equal(24, Regex.Count(await Page.Locator("pre.codigo").TextContentAsync() ?? "", "Escenario:"));
    }

    [Fact]
    public async Task CampoSinRango_AvisaYAlCompletarloMuestraLosCasos()
    {
        await AbrirAsync();

        await EscribirAsync("Nombre del campo 1", "edad");

        await Expect(Page.GetByText(AvisoDeRango)).ToBeVisibleAsync();
        await Expect(Page.GetByText("Corrige los avisos de arriba para ver los casos.")).ToBeVisibleAsync();

        await EscribirAsync("Mínimo del campo 1", "18");
        await EscribirAsync("Máximo del campo 1", "65");

        await Expect(Page.GetByText(AvisoDeRango)).ToBeHiddenAsync();
        await Expect(Page.Locator(".resumen")).ToHaveTextAsync("12 casos: 5 se aceptan y 7 se rechazan.");
    }

    [Fact]
    public async Task DescargarElCsv_TraeElEncabezadoYUnaFilaPorCaso()
    {
        await AbrirAsync();
        await Boton("Registro de usuario").ClickAsync();

        var descarga = await Page.RunAndWaitForDownloadAsync(() => Boton("Descargar en CSV").ClickAsync());

        var lineas = await File.ReadAllLinesAsync(await descarga.PathAsync(), Encoding.UTF8);
        Assert.Equal("registro-de-usuario.csv", descarga.SuggestedFilename);
        Assert.Equal("Código;Descripción;Técnica;edad;nombre;tipo de cliente;Resultado esperado", lineas[0]);
        Assert.Equal(1 + 24, lineas.Length);
    }

    [Fact]
    public async Task ElegirIngles_CambiaLaInterfazYSeMantieneAlRecargar()
    {
        await AbrirAsync();
        var tituloEnIngles = Page.GetByRole(AriaRole.Heading, new() { Name = "Story fields", Level = 1 });

        await Boton("English (EN)").ClickAsync();
        await Expect(tituloEnIngles).ToBeVisibleAsync(new() { Timeout = EsperaDeCarga });
        await Page.ReloadAsync();

        await Expect(tituloEnIngles).ToBeVisibleAsync(new() { Timeout = EsperaDeCarga });
        await Expect(Page.Locator("html")).ToHaveAttributeAsync("lang", "en");
    }
}
