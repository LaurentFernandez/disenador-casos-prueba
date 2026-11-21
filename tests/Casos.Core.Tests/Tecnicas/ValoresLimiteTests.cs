using Casos.Core.Modelo;
using Casos.Core.Tecnicas;

namespace Casos.Core.Tests.Tecnicas;

public class ValoresLimiteTests
{
    [Fact]
    public void Generar_RangoNormal_DevuelveCadaBordeConSusVecinos()
    {
        var valores = ValoresLimite.Generar(18, 65);

        Assert.Equal(new long[] { 17, 18, 19, 64, 65, 66 }, valores);
    }

    [Fact]
    public void Generar_RangoDeUnSoloValor_NoRepiteValores()
    {
        var valores = ValoresLimite.Generar(5, 5);

        Assert.Equal(new long[] { 4, 5, 6 }, valores);
    }

    [Fact]
    public void Generar_RangoDeDosValores_NoRepiteValores()
    {
        var valores = ValoresLimite.Generar(1, 2);

        Assert.Equal(new long[] { 0, 1, 2, 3 }, valores);
    }

    [Fact]
    public void Generar_ExtremosDeInt_NoSeDesborda()
    {
        var valores = ValoresLimite.Generar(int.MinValue, int.MaxValue);

        Assert.Equal((long)int.MinValue - 1, valores[0]);
        Assert.Equal((long)int.MaxValue + 1, valores[^1]);
    }

    [Fact]
    public void Generar_MinimoMayorQueMaximo_LanzaExcepcion()
    {
        Assert.Throws<ArgumentException>(() => ValoresLimite.Generar(10, 1));
    }

    [Fact]
    public void GenerarLongitudes_LongitudConMinimoYMaximo_DevuelveCadaBordeConSusVecinos()
    {
        var longitudes = ValoresLimite.GenerarLongitudes(new CampoTexto("nombre", true, 3, 50));

        Assert.Equal(new long[] { 2, 3, 4, 49, 50, 51 }, longitudes);
    }

    [Fact]
    public void GenerarLongitudes_LongitudMinimaCero_NoDevuelveLongitudesNegativas()
    {
        var longitudes = ValoresLimite.GenerarLongitudes(new CampoTexto("comentario", false, 0, 10));

        Assert.Equal(new long[] { 0, 1, 9, 10, 11 }, longitudes);
    }
}
