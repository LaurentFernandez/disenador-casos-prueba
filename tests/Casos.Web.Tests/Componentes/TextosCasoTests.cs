using Casos.Core;
using Casos.Core.Modelo;
using Casos.Web.Componentes;

namespace Casos.Web.Tests.Componentes;

public class TextosCasoTests() : PruebaEnIdioma("es")
{
    private readonly IReadOnlyList<CasoPrueba> tabla = CasosDeLaRegla();

    [Fact]
    public void Descripcion_CasoDeRegla_DiceLaOpcionYSiElCampoObligatorioTieneValor()
    {
        Assert.Equal(
            new[]
            {
                "Tipo de cliente: Persona y NIT sin valor",
                "Tipo de cliente: Persona y NIT con valor",
                "Tipo de cliente: Empresa y NIT sin valor",
                "Tipo de cliente: Empresa y NIT con valor",
            },
            tabla.Select(TextosCaso.Descripcion));
    }

    [Fact]
    public void Resultado_CasoDeReglaQueSeRechaza_RechazaElValorDelCampoObligatorio()
    {
        Assert.Equal(
            new[] { "Acepta los datos", "Acepta los datos", "Rechaza el valor de NIT", "Acepta los datos" },
            tabla.Select(TextosCaso.Resultado));
    }

    [Fact]
    public void ValorProbado_CasoDeRegla_MuestraLosDosCampos()
    {
        Assert.Equal("tipo de cliente: Empresa · NIT: vacío", TextosCaso.ValorProbado(tabla[2]));
    }

    [Fact]
    public void NombreTecnica_TablaDeDecision_TieneSuNombreCortoYLargo()
    {
        Assert.Equal("Decisión", TextosCaso.NombreTecnica(Tecnica.TablaDecision));
        Assert.Equal("Tabla de decisión", TextosCaso.NombreLargoTecnica(Tecnica.TablaDecision));
    }

    private static IReadOnlyList<CasoPrueba> CasosDeLaRegla()
    {
        var historia = new Historia("Registro de cliente");
        historia.AgregarCampo(new CampoLista("tipo de cliente", true, ["Persona", "Empresa"]));
        historia.AgregarCampo(new CampoTexto("NIT", false, 9, 10));
        historia.AgregarRegla(new ReglaObligatorio("tipo de cliente", "Empresa", "NIT"));
        return GeneradorCasos.Generar(historia).Where(caso => caso.Tecnica == Tecnica.TablaDecision).ToList();
    }
}
