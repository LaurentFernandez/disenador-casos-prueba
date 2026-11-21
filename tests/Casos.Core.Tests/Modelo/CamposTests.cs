using Casos.Core.Modelo;

namespace Casos.Core.Tests.Modelo;

public class CamposTests
{
    [Fact]
    public void CampoEntero_MinimoMayorQueMaximo_LanzaExcepcion()
    {
        Assert.Throws<ArgumentException>(() => new CampoEntero("edad", true, Minimo: 10, Maximo: 1));
    }

    [Fact]
    public void CampoEntero_MinimoIgualAlMaximo_SeCrea()
    {
        var campo = new CampoEntero("cantidad", true, Minimo: 5, Maximo: 5);

        Assert.Equal(5, campo.Minimo);
        Assert.Equal(5, campo.Maximo);
    }

    [Fact]
    public void CampoTexto_LongitudMinimaMayorQueMaxima_LanzaExcepcion()
    {
        Assert.Throws<ArgumentException>(() => new CampoTexto("nombre", true, LongitudMinima: 10, LongitudMaxima: 5));
    }

    [Fact]
    public void CampoLista_SinOpciones_LanzaExcepcion()
    {
        Assert.Throws<ArgumentException>(() => new CampoLista("tipo de cliente", true, []));
    }
}
