using Casos.Core.Modelo;
using Casos.Core.Tecnicas;

namespace Casos.Core.Tests.Tecnicas;

public class TablaDecisionTests
{
    private static readonly CampoTexto Nit = new("NIT", false, 9, 10);
    private static readonly string NitConValor = new('a', 9);

    [Fact]
    public void Generar_ReglaDeHU11_DaLasCuatroCombinacionesDeLaTabla()
    {
        var tipoDeCliente = new CampoLista("tipo de cliente", true, ["Persona", "Empresa"]);

        var tabla = TablaDecision.Generar(tipoDeCliente, "Empresa", Nit);

        Assert.Equal(new[]
        {
            ("Persona", "", true),
            ("Persona", NitConValor, true),
            ("Empresa", "", false),
            ("Empresa", NitConValor, true),
        }, tabla.Select(combinacion => (combinacion.ValorCondicion, combinacion.ClaseObligatorio.ValorEjemplo, combinacion.EsValida)));
    }

    [Fact]
    public void Generar_ListaConMasOpciones_UsaUnaSolaOpcionRepresentativaParaElNo()
    {
        var tipoDeCliente = new CampoLista("tipo de cliente", true, ["Persona", "Empresa", "Gobierno"]);

        var tabla = TablaDecision.Generar(tipoDeCliente, "Empresa", Nit);

        Assert.Equal(new[] { "Persona", "Persona", "Empresa", "Empresa" }, tabla.Select(combinacion => combinacion.ValorCondicion));
    }

    [Fact]
    public void Generar_OpcionDeLaCondicionPrimeroEnLaLista_SigueElOrdenDeLaLista()
    {
        var tipoDeCliente = new CampoLista("tipo de cliente", true, ["Empresa", "Persona"]);

        var tabla = TablaDecision.Generar(tipoDeCliente, "Empresa", Nit);

        Assert.Equal(new[] { "Empresa", "Empresa", "Persona", "Persona" }, tabla.Select(combinacion => combinacion.ValorCondicion));
        Assert.False(tabla[0].EsValida);
    }

    [Fact]
    public void Generar_ListaDeUnaSolaOpcion_SoloDaLasCombinacionesQueCumplenLaCondicion()
    {
        var tipoDeCliente = new CampoLista("tipo de cliente", true, ["Empresa"]);

        var tabla = TablaDecision.Generar(tipoDeCliente, "Empresa", Nit);

        Assert.Equal(new[] { ("", false), (NitConValor, true) }, tabla.Select(combinacion => (combinacion.ClaseObligatorio.ValorEjemplo, combinacion.EsValida)));
    }

    [Fact]
    public void Generar_CampoObligatorioEntero_ConValorUsaUnValorDentroDelRango()
    {
        var tipoDeCliente = new CampoLista("tipo de cliente", true, ["Persona", "Empresa"]);

        var tabla = TablaDecision.Generar(tipoDeCliente, "Empresa", new CampoEntero("empleados", false, 1, 99));

        Assert.Equal(new[] { TipoClase.Vacio, TipoClase.DentroDelRango }, tabla.Take(2).Select(combinacion => combinacion.ClaseObligatorio.Tipo));
        Assert.Equal("50", tabla[1].ClaseObligatorio.ValorEjemplo);
    }
}
