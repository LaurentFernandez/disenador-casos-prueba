using Casos.Core.Modelo;

namespace Casos.Core.Tests.Modelo;

public class CamposTests
{
    [Fact]
    public void CampoEntero_MinimoMayorQueMaximo_IndicaRangoInvalido()
    {
        var excepcion = Assert.Throws<CampoInvalidoException>(() => new CampoEntero("edad", true, Minimo: 10, Maximo: 1));

        Assert.Equal(ErrorCampo.RangoInvalido, excepcion.Error);
    }

    [Fact]
    public void CampoEntero_MinimoIgualAlMaximo_SeCrea()
    {
        var campo = new CampoEntero("cantidad", true, Minimo: 5, Maximo: 5);

        Assert.Equal(5, campo.Minimo);
        Assert.Equal(5, campo.Maximo);
    }

    [Fact]
    public void CampoTexto_LongitudMinimaMayorQueMaxima_IndicaLongitudInvalida()
    {
        var excepcion = Assert.Throws<CampoInvalidoException>(() => new CampoTexto("nombre", true, LongitudMinima: 10, LongitudMaxima: 5));

        Assert.Equal(ErrorCampo.LongitudInvalida, excepcion.Error);
    }

    [Fact]
    public void CampoLista_SinOpciones_IndicaListaSinOpciones()
    {
        var excepcion = Assert.Throws<CampoInvalidoException>(() => new CampoLista("tipo de cliente", true, []));

        Assert.Equal(ErrorCampo.ListaSinOpciones, excepcion.Error);
    }
}
