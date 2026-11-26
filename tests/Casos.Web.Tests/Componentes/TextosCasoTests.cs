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

    [Fact]
    public void Descripcion_CasoDePares_JuntaLasOpcionesQueCombina()
    {
        Assert.Equal("Chrome · Windows · español", TextosCaso.Descripcion(CasosPorPares()[0]));
    }

    [Fact]
    public void ValorProbado_CasoDePares_MuestraCadaCampoCombinado()
    {
        Assert.Equal("navegador: Chrome · sistema: Windows · idioma: español", TextosCaso.ValorProbado(CasosPorPares()[0]));
    }

    [Fact]
    public void NombreTecnica_Pares_TieneSuNombreCortoYLargo()
    {
        Assert.Equal("Pares", TextosCaso.NombreTecnica(Tecnica.Pares));
        Assert.Equal("Combinación por pares", TextosCaso.NombreLargoTecnica(Tecnica.Pares));
    }

    [Fact]
    public void TotalPares_TresCamposDeHU12_DiceSeisDeDoce()
    {
        var historia = HistoriaPorPares();

        Assert.Equal("Combinaciones por pares: 6 de 12 posibles.", TextosCaso.TotalPares(historia, GeneradorCasos.Generar(historia)));
    }

    [Fact]
    public void TotalPares_VeinteListasDeDiezOpciones_MuestraElTotalQueNoCabeEnUnLong()
    {
        var historia = new Historia("Configuración");
        foreach (var numero in Enumerable.Range(1, 20))
        {
            historia.AgregarCampo(new CampoLista($"opción {numero}", true, Enumerable.Range(0, 10).Select(valor => $"{valor}").ToList()));
        }

        Assert.Equal("Combinaciones por pares: 0 de 100.000.000.000.000.000.000 posibles.", TextosCaso.TotalPares(historia, []));
    }

    [Fact]
    public void Reglas_CampoFecha_DiceSuRangoDeFechas()
    {
        Assert.Equal("Fecha, obligatorio, del 2026-01-01 al 2026-12-31", TextosCaso.Reglas(FechaDeReserva()));
    }

    [Fact]
    public void Descripcion_CasosDeUnaFecha_DicenSuClaseOSuLimiteComoEnHU17()
    {
        var historia = new Historia("Reserva");
        historia.AgregarCampo(FechaDeReserva());

        Assert.Equal(
            new[]
            {
                "Todos los valores válidos",
                "Fecha de reserva anterior al 2026-01-01",
                "Fecha de reserva posterior al 2026-12-31",
                "Fecha de reserva que no existe en el calendario",
                "Fecha de reserva que no es una fecha",
                "Fecha de reserva sin valor",
                "Fecha de reserva el día antes del mínimo",
                "Fecha de reserva en el mínimo",
                "Fecha de reserva el día después del mínimo",
                "Fecha de reserva el día antes del máximo",
                "Fecha de reserva en el máximo",
                "Fecha de reserva el día después del máximo",
            },
            GeneradorCasos.Generar(historia).Select(TextosCaso.Descripcion));
    }

    private static CampoFecha FechaDeReserva() => new("fecha de reserva", true, new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31));

    private static IReadOnlyList<CasoPrueba> CasosPorPares() =>
        GeneradorCasos.Generar(HistoriaPorPares()).Where(caso => caso.Tecnica == Tecnica.Pares).ToList();

    private static Historia HistoriaPorPares()
    {
        var historia = new Historia("Compatibilidad");
        historia.AgregarCampo(new CampoEntero("edad", true, 18, 65));
        historia.AgregarCampo(new CampoLista("navegador", true, ["Chrome", "Firefox", "Edge"]));
        historia.AgregarCampo(new CampoLista("sistema", true, ["Windows", "macOS"]));
        historia.AgregarCampo(new CampoLista("idioma", true, ["español", "inglés"]));
        historia.CombinarPorPares();
        return historia;
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
