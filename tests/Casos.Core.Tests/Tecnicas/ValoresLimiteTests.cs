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

    [Fact]
    public void GenerarFechas_FechaDeHU17_DevuelveCadaExtremoConElDiaAnteriorYElSiguiente()
    {
        var fechas = ValoresLimite.GenerarFechas(new CampoFecha("fecha de reserva", true, new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31)));

        Assert.Equal(
            new[] { "2025-12-31", "2026-01-01", "2026-01-02", "2026-12-30", "2026-12-31", "2027-01-01" },
            fechas.Select(fecha => fecha.ToString("yyyy-MM-dd")));
    }

    [Fact]
    public void GenerarFechas_FinDeFebreroDeUnAnioBisiesto_CuentaEl29()
    {
        var fechas = ValoresLimite.GenerarFechas(new CampoFecha("fecha", true, new DateOnly(2024, 2, 28), new DateOnly(2024, 3, 1)));

        Assert.Equal(
            new[] { "2024-02-27", "2024-02-28", "2024-02-29", "2024-03-01", "2024-03-02" },
            fechas.Select(fecha => fecha.ToString("yyyy-MM-dd")));
    }

    [Fact]
    public void GenerarFechas_ExtremosDelCalendario_NoSeSaleDeEl()
    {
        var fechas = ValoresLimite.GenerarFechas(new CampoFecha("fecha", true, DateOnly.MinValue, DateOnly.MaxValue));

        Assert.Equal(DateOnly.MinValue, fechas[0]);
        Assert.Equal(DateOnly.MaxValue, fechas[^1]);
        Assert.Equal(4, fechas.Count);
    }
}
