using Casos.Core.Modelo;
using Casos.Core.Tecnicas;

namespace Casos.Core.Tests.Tecnicas;

public class ParticionEquivalenciaTests
{
    [Fact]
    public void Generar_EnteroObligatorio_DevuelveLasClasesConSuValorDeEjemplo()
    {
        var clases = ParticionEquivalencia.Generar(new CampoEntero("edad", true, 18, 65));

        Assert.Equal(new ClaseEquivalencia[]
        {
            new(TipoClase.DentroDelRango, true, "41"),
            new(TipoClase.MenorQueMinimo, false, "17"),
            new(TipoClase.MayorQueMaximo, false, "66"),
            new(TipoClase.ConDecimales, false, "18.5"),
            new(TipoClase.NoEsNumero, false, "abc"),
            new(TipoClase.Vacio, false, ""),
        }, clases);
    }

    [Fact]
    public void Generar_CampoOpcional_LaClaseVacioEsValida()
    {
        var clases = ParticionEquivalencia.Generar(new CampoEntero("edad", false, 18, 65));

        Assert.True(clases.Single(clase => clase.Tipo == TipoClase.Vacio).EsValida);
    }

    [Fact]
    public void Generar_EnteroNegativo_ElDecimalQuedaDentroDelRango()
    {
        var clases = ParticionEquivalencia.Generar(new CampoEntero("temperatura", true, -10, -5));

        Assert.Equal("-9.5", clases.Single(clase => clase.Tipo == TipoClase.ConDecimales).ValorEjemplo);
    }

    [Fact]
    public void Generar_EnteroEnExtremosDeInt_NoSeDesborda()
    {
        var clases = ParticionEquivalencia.Generar(new CampoEntero("valor", true, int.MinValue, int.MaxValue));

        Assert.Equal("-2147483649", clases.Single(clase => clase.Tipo == TipoClase.MenorQueMinimo).ValorEjemplo);
        Assert.Equal("2147483648", clases.Single(clase => clase.Tipo == TipoClase.MayorQueMaximo).ValorEjemplo);
    }

    [Fact]
    public void Generar_TextoObligatorio_DevuelveLasClasesConSuValorDeEjemplo()
    {
        var clases = ParticionEquivalencia.Generar(new CampoTexto("nombre", true, 3, 50));

        Assert.Equal(new ClaseEquivalencia[]
        {
            new(TipoClase.DentroDelRango, true, new string('a', 26)),
            new(TipoClase.MenorQueMinimo, false, new string('a', 2)),
            new(TipoClase.MayorQueMaximo, false, new string('a', 51)),
            new(TipoClase.Vacio, false, ""),
        }, clases);
    }

    [Fact]
    public void Generar_TextoConLongitudMinimaUno_NoGeneraClaseMasCorta()
    {
        var clases = ParticionEquivalencia.Generar(new CampoTexto("nombre", true, 1, 50));

        Assert.DoesNotContain(clases, clase => clase.Tipo == TipoClase.MenorQueMinimo);
    }

    [Fact]
    public void Generar_ListaObligatoria_DevuelveUnaClasePorOpcionMasLasInvalidas()
    {
        var clases = ParticionEquivalencia.Generar(new CampoLista("tipo de cliente", true, ["Persona", "Empresa"]));

        Assert.Equal(new ClaseEquivalencia[]
        {
            new(TipoClase.Opcion, true, "Persona"),
            new(TipoClase.Opcion, true, "Empresa"),
            new(TipoClase.FueraDeLista, false, "Opción no válida"),
            new(TipoClase.Vacio, false, ""),
        }, clases);
    }
}
