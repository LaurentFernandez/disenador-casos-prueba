using Casos.Web.Componentes;

namespace Casos.Web.Tests.Componentes;

public class HistoriaGuardadaTests
{
    [Fact]
    public void AJsonYDesdeJson_ConservanLaHistoriaConSusCamposSuReglaYLaCasillaComoEnHU15()
    {
        var original = new HistoriaGuardada(
            "Registro de cliente",
            [
                new() { Nombre = "tipo de cliente", Tipo = TipoCampo.Lista, Opciones = "Persona, Empresa" },
                new() { Nombre = "NIT", Tipo = TipoCampo.Texto, Obligatorio = false, Minimo = 9, Maximo = 10 },
                new() { Nombre = "canal", Tipo = TipoCampo.Lista, Opciones = "Web, Tienda" },
                new() { Nombre = "fecha de visita", Tipo = TipoCampo.Fecha, Obligatorio = false, FechaMinima = new DateOnly(2026, 1, 1), FechaMaxima = new DateOnly(2026, 12, 31) },
            ],
            [new() { CampoCondicion = "tipo de cliente", Opcion = "Empresa", CampoObligatorio = "NIT" }],
            PorPares: true,
            CambiosPropios: true);

        var restaurada = HistoriaGuardada.DesdeJson(original.AJson())!;

        Assert.Equal("Registro de cliente", restaurada.Nombre);
        Assert.Equal(DatosDe(original.Filas), DatosDe(restaurada.Filas));
        var regla = Assert.Single(restaurada.Reglas);
        Assert.Equal(("tipo de cliente", "Empresa", "NIT"), (regla.CampoCondicion, regla.Opcion, regla.CampoObligatorio));
        Assert.True(restaurada.PorPares);
        Assert.True(restaurada.CambiosPropios);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("no es JSON")]
    [InlineData("{}")]
    [InlineData("""{"nombre":"Registro","filas":[{"nombre":"hora","tipo":"Hora"}]}""")]
    public void DesdeJson_NadaGuardadoOQueNoSeEntiende_DevuelveNull(string? json)
    {
        Assert.Null(HistoriaGuardada.DesdeJson(json));
    }

    [Fact]
    public void DesdeJson_TextosQueFaltan_LosCompletaParaQueLaPaginaNoFalle()
    {
        var restaurada = HistoriaGuardada.DesdeJson("""
            {"filas":[{"nombre":null,"tipo":"Lista","opciones":null},null],"reglas":[{"campoCondicion":null},null]}
            """)!;

        Assert.Equal("", restaurada.Nombre);
        var fila = Assert.Single(restaurada.Filas);
        Assert.Equal(("", ""), (fila.Nombre, fila.Opciones));
        Assert.True(Assert.Single(restaurada.Reglas).EstaVacia);
    }

    private static IEnumerable<(string, TipoCampo, bool, int?, int?, DateOnly?, DateOnly?, string)> DatosDe(IEnumerable<FilaCampo> filas) =>
        filas.Select(fila => (fila.Nombre, fila.Tipo, fila.Obligatorio, fila.Minimo, fila.Maximo, fila.FechaMinima, fila.FechaMaxima, fila.Opciones));
}
