using System.Text;
using Microsoft.Playwright;

namespace Casos.E2E.Tests;

[Collection(ColeccionAplicacion.Nombre)]
public class ExportarImportarTests(AplicacionEnMarcha aplicacion) : PruebaEnNavegador(aplicacion)
{
    [Fact]
    public async Task ExportarEImportar_RecuperaLaHistoriaYPideConfirmarSobreCamposPropiosComoEnHU16()
    {
        var preguntas = 0;
        Page.Dialog += async (_, dialogo) =>
        {
            preguntas++;
            await dialogo.AcceptAsync();
        };
        await AbrirAsync();
        await CargarCarritoConReglaAsync();

        var descarga = await Page.RunAndWaitForDownloadAsync(() => Boton("Exportar la historia").ClickAsync());
        Assert.Equal("carrito-de-compras.json", descarga.SuggestedFilename);
        await Boton("Empezar de nuevo").ClickAsync();
        await EscribirEdadAsync();

        await Etiqueta("Importar una historia").SetInputFilesAsync(await descarga.PathAsync());

        await Expect(Page.Locator("#nombre-historia")).ToHaveValueAsync("Carrito de compras");
        await Expect(Etiqueta("Campo obligatorio de la regla 1")).ToHaveValueAsync("cupón");
        await Expect(Resumen).ToHaveTextAsync(ResumenCarritoConRegla);
        // Una al empezar de nuevo, porque la regla era propia, y otra al importar sobre la edad escrita a mano.
        Assert.Equal(2, preguntas);
    }

    [Fact]
    public async Task ImportarUnArchivoQueNoEsUnaHistoria_AvisaYNoCambiaLaHojaComoEnHU16()
    {
        await AbrirAsync();
        await EscribirEdadAsync();

        await Etiqueta("Importar una historia").SetInputFilesAsync(new FilePayload
        {
            Name = "otro.json",
            MimeType = "application/json",
            Buffer = Encoding.UTF8.GetBytes("""{"campos":[]}"""),
        });

        await Expect(Page.GetByRole(AriaRole.Alert)).ToHaveTextAsync("El archivo no es una historia exportada por esta herramienta, o está dañado.");
        await Expect(Etiqueta("Nombre del campo 1")).ToHaveValueAsync("edad");
        await Expect(Resumen).ToHaveTextAsync(ResumenEdadDe18A65);
    }
}
