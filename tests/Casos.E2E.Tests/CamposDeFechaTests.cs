namespace Casos.E2E.Tests;

[Collection(ColeccionAplicacion.Nombre)]
public class CamposDeFechaTests(AplicacionEnMarcha aplicacion) : PruebaEnNavegador(aplicacion)
{
    [Fact]
    public async Task CampoDeFecha_GeneraSusCasosYAvisaSiElRangoQuedaAlRevesComoEnHU17()
    {
        await AbrirAsync();
        await Etiqueta("Tipo del campo 1").SelectOptionAsync("Fecha");
        await EscribirAsync("Nombre del campo 1", "fecha de reserva");
        await Expect(Page.GetByText("Elige la fecha mínima y la máxima.")).ToBeVisibleAsync();

        await EscribirAsync("Mínimo del campo 1", "2026-01-01");
        await EscribirAsync("Máximo del campo 1", "2026-12-31");

        // Seis casos de partición (uno válido) y seis de valores límite (cuatro válidos).
        await Expect(Resumen).ToHaveTextAsync("12 casos: 5 se aceptan y 7 se rechazan.");
        await Expect(Page.Locator("table.matriz")).ToContainTextAsync("Fecha de reserva que no existe en el calendario");
        await Expect(Page.Locator("table.matriz")).ToContainTextAsync("2026-02-29");

        await EscribirAsync("Mínimo del campo 1", "2027-01-01");

        await Expect(Page.GetByText("La fecha mínima no puede ser posterior a la máxima.")).ToBeVisibleAsync();
        await Expect(Page.GetByText("Corrige los avisos de arriba para ver los casos.")).ToBeVisibleAsync();
    }
}
