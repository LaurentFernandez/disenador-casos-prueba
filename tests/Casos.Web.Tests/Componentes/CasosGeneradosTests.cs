using Bunit;
using Casos.Core;
using Casos.Core.Modelo;
using Casos.Web.Componentes;

namespace Casos.Web.Tests.Componentes;

public class CasosGeneradosTests() : PruebaEnIdioma("es"), IDisposable
{
    private readonly BunitContext contexto = new();

    [Fact]
    public void SinReglasNiPares_LaLeyendaSoloTraeLasTecnicasDeSiempre()
    {
        var historia = new Historia("Registro de usuario");
        historia.AgregarCampo(new CampoEntero("edad", true, 18, 65));

        Assert.Equal(
            new[] { "Válido", "Inválido", "Partición de equivalencia", "Valor límite" },
            Leyenda(historia));
    }

    [Fact]
    public void ConReglaYPares_LaLeyendaAgregaLaTablaDeDecisionYLaCombinacionPorPares()
    {
        var historia = new Historia("Registro de cliente");
        historia.AgregarCampo(new CampoLista("tipo de cliente", true, ["Persona", "Empresa"]));
        historia.AgregarCampo(new CampoLista("canal", true, ["Web", "Tienda"]));
        historia.AgregarCampo(new CampoTexto("NIT", false, 9, 10));
        historia.AgregarRegla(new ReglaObligatorio("tipo de cliente", "Empresa", "NIT"));
        historia.CombinarPorPares();

        Assert.Equal(
            new[] { "Válido", "Inválido", "Partición de equivalencia", "Valor límite", "Tabla de decisión", "Combinación por pares" },
            Leyenda(historia));
    }

    public void Dispose() => contexto.Dispose();

    private IEnumerable<string> Leyenda(Historia historia) =>
        contexto.Render<CasosGenerados>(parametros => parametros
                .Add(casos => casos.Historia, historia)
                .Add(casos => casos.Casos, GeneradorCasos.Generar(historia)))
            .FindAll(".leyenda li")
            .Select(elemento => elemento.TextContent.Trim());
}
