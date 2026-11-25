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
    public void CampoTexto_LongitudMinimaNegativa_IndicaLongitudNegativa()
    {
        var excepcion = Assert.Throws<CampoInvalidoException>(() => new CampoTexto("nombre", true, LongitudMinima: -1, LongitudMaxima: 5));

        Assert.Equal(ErrorCampo.LongitudNegativa, excepcion.Error);
    }

    [Fact]
    public void CampoTexto_LongitudMaximaSobreElTope_IndicaLongitudExcesiva()
    {
        var excepcion = Assert.Throws<CampoInvalidoException>(() => new CampoTexto("comentario", false, 0, CampoTexto.LongitudMaximaPermitida + 1));

        Assert.Equal(ErrorCampo.LongitudExcesiva, excepcion.Error);
    }

    [Fact]
    public void CampoTexto_LongitudMaximaEnElTope_SeCrea()
    {
        var campo = new CampoTexto("comentario", false, 0, CampoTexto.LongitudMaximaPermitida);

        Assert.Equal(CampoTexto.LongitudMaximaPermitida, campo.LongitudMaxima);
    }

    [Fact]
    public void CampoLista_SinOpciones_IndicaListaSinOpciones()
    {
        var excepcion = Assert.Throws<CampoInvalidoException>(() => new CampoLista("tipo de cliente", true, []));

        Assert.Equal(ErrorCampo.ListaSinOpciones, excepcion.Error);
    }

    [Fact]
    public void CampoFecha_MinimoPosteriorAlMaximo_IndicaRangoDeFechasInvalidoComoEnHU17()
    {
        var excepcion = Assert.Throws<CampoInvalidoException>(() =>
            new CampoFecha("fecha de reserva", true, new DateOnly(2026, 12, 31), new DateOnly(2026, 1, 1)));

        Assert.Equal(ErrorCampo.RangoDeFechasInvalido, excepcion.Error);
    }

    [Fact]
    public void CampoFecha_MinimoIgualAlMaximo_SeCrea()
    {
        var dia = new DateOnly(2026, 6, 15);

        Assert.Equal(dia, new CampoFecha("fecha de entrega", true, dia, dia).Maximo);
    }

    [Theory]
    [InlineData(new[] { "Persona", "Empresa" }, "Opción no válida")]
    [InlineData(new[] { "Persona", "Opción no válida" }, "Opción no válida 2")]
    [InlineData(new[] { "OPCIÓN NO VÁLIDA" }, "Opción no válida 2")]
    [InlineData(new[] { "Opción no válida", "opción no válida 2" }, "Opción no válida 3")]
    public void CampoLista_ValorQueNoEsOpcion_AgregaUnNumeroSiElPreferidoYaEsUnaOpcion(string[] opciones, string esperado)
    {
        Assert.Equal(esperado, new CampoLista("tipo de cliente", true, opciones).ValorQueNoEsOpcion("Opción no válida"));
    }
}
