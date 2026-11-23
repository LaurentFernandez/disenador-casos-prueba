using Bunit;
using Casos.Core;
using Casos.Core.Modelo;
using Casos.Web.Componentes;

namespace Casos.Web.Tests.Componentes;

public class VistaMatrizTests() : PruebaEnIdioma("es"), IDisposable
{
    private readonly BunitContext contexto = new();

    [Fact]
    public void CasoDeUnSoloCampo_ResaltaSoloLaCeldaDeEseCampo()
    {
        var historia = HistoriaCon(new CampoEntero("edad", true, 18, 65), new CampoTexto("nombre", true, 3, 50));

        var matriz = Dibujar(historia, GeneradorCasos.Generar(historia));

        // CP-002 prueba una edad menor que el mínimo y deja el nombre con su valor por defecto.
        Assert.Equal(new[] { "valor valor--invalido", "valor" }, ClasesDeLaFila(matriz, "CP-002"));
    }

    [Fact]
    public void CasoPorPares_ResaltaLosCamposQueCombinaYNoLosDemas()
    {
        var historia = HistoriaCon(
            new CampoEntero("edad", true, 18, 65),
            new CampoLista("sistema", true, ["Windows", "macOS"]),
            new CampoLista("idioma", true, ["español", "inglés"]));
        historia.CombinarPorPares();
        var casos = GeneradorCasos.Generar(historia);

        var matriz = Dibujar(historia, casos);

        Assert.Equal(
            new[] { "valor", "valor valor--valido", "valor valor--valido" },
            ClasesDeLaFila(matriz, casos.Last(caso => caso.Tecnica == Tecnica.Pares).Codigo));
    }

    public void Dispose() => contexto.Dispose();

    private IRenderedComponent<VistaMatriz> Dibujar(Historia historia, IReadOnlyList<CasoPrueba> casos) =>
        contexto.Render<VistaMatriz>(parametros => parametros
            .Add(vista => vista.Campos, historia.Campos)
            .Add(vista => vista.Casos, casos));

    private static IEnumerable<string?> ClasesDeLaFila(IRenderedComponent<VistaMatriz> matriz, string codigo) =>
        matriz.FindAll("tbody tr")
            .Single(fila => fila.QuerySelector("th")!.TextContent == codigo)
            .QuerySelectorAll("td.valor")
            .Select(celda => celda.ClassName);

    private static Historia HistoriaCon(params Campo[] campos)
    {
        var historia = new Historia("Registro de usuario");
        foreach (var campo in campos)
        {
            historia.AgregarCampo(campo);
        }

        return historia;
    }
}
