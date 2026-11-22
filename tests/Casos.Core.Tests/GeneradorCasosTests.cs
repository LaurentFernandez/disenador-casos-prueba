using Casos.Core.Modelo;
using static Casos.Core.Modelo.PosicionLimite;
using static Casos.Core.Modelo.Tecnica;
using static Casos.Core.Modelo.TipoClase;

namespace Casos.Core.Tests;

public class GeneradorCasosTests
{
    private static readonly string NombreValido = new('a', 26);
    private static readonly string NitConValor = new('a', 9);

    [Fact]
    public void Generar_UnCampoEntero_DevuelveLosCasosEnOrdenConCodigosConsecutivos()
    {
        var casos = GeneradorCasos.Generar(HistoriaCon(new CampoEntero("edad", true, 18, 65)));

        Assert.Equal(new (string, Tecnica, string, TipoClase?, bool)[]
        {
            ("CP-001", ParticionEquivalencia, "41", null, true),
            ("CP-002", ParticionEquivalencia, "17", MenorQueMinimo, false),
            ("CP-003", ParticionEquivalencia, "66", MayorQueMaximo, false),
            ("CP-004", ParticionEquivalencia, "18.5", ConDecimales, false),
            ("CP-005", ParticionEquivalencia, "abc", NoEsNumero, false),
            ("CP-006", ParticionEquivalencia, "", Vacio, false),
            ("CP-007", ValoresLimite, "17", MenorQueMinimo, false),
            ("CP-008", ValoresLimite, "18", DentroDelRango, true),
            ("CP-009", ValoresLimite, "19", DentroDelRango, true),
            ("CP-010", ValoresLimite, "64", DentroDelRango, true),
            ("CP-011", ValoresLimite, "65", DentroDelRango, true),
            ("CP-012", ValoresLimite, "66", MayorQueMaximo, false),
        }, casos.Select(caso => (caso.Codigo, caso.Tecnica, caso.Datos["edad"], caso.ClaseProbada, caso.EsValido)));
    }

    [Fact]
    public void Generar_CasosDeValoresLimite_IndicanDondeCaeElValor()
    {
        var casos = GeneradorCasos.Generar(HistoriaCon(new CampoEntero("edad", true, 18, 65)));

        Assert.Equal(
            new PosicionLimite?[] { DebajoDelMinimo, EnElMinimo, EncimaDelMinimo, DebajoDelMaximo, EnElMaximo, EncimaDelMaximo },
            casos.Where(caso => caso.Tecnica == ValoresLimite).Select(caso => caso.Posicion));
        Assert.All(casos.Where(caso => caso.Tecnica == ParticionEquivalencia), caso => Assert.Null(caso.Posicion));
    }

    [Fact]
    public void Generar_RangoDeUnSoloValor_ElValorQuedaEnElMinimo()
    {
        var casos = GeneradorCasos.Generar(HistoriaCon(new CampoEntero("cantidad", true, 5, 5)));

        Assert.Equal(
            new PosicionLimite?[] { DebajoDelMinimo, EnElMinimo, EncimaDelMaximo },
            casos.Where(caso => caso.Tecnica == ValoresLimite).Select(caso => caso.Posicion));
    }

    [Fact]
    public void Generar_ValorInvalido_UsaUnNombreValidoYRechazaLaEdad()
    {
        var casos = GeneradorCasos.Generar(HistoriaConEdadYNombre());

        var casosConEdad17 = casos.Where(caso => caso.Datos["edad"] == "17").ToList();

        Assert.Equal(2, casosConEdad17.Count);
        Assert.All(casosConEdad17, caso =>
        {
            Assert.Equal(NombreValido, caso.Datos["nombre"]);
            Assert.Equal("edad", caso.CampoProbado?.Nombre);
            Assert.False(caso.EsValido);
        });
    }

    [Fact]
    public void Generar_CadaCaso_CambiaSoloElCampoProbado()
    {
        var casos = GeneradorCasos.Generar(HistoriaConEdadYNombre());
        var porDefecto = casos[0].Datos;

        Assert.All(casos, caso =>
            Assert.All(caso.Datos.Where(dato => dato.Key != caso.CampoProbado?.Nombre), dato =>
                Assert.Equal(porDefecto[dato.Key], dato.Value)));
    }

    [Fact]
    public void Generar_PrimerCaso_TieneTodosLosValoresPorDefectoYEsValido()
    {
        var primero = GeneradorCasos.Generar(HistoriaConEdadYNombre())[0];

        Assert.Equal(ParticionEquivalencia, primero.Tecnica);
        Assert.Null(primero.CampoProbado);
        Assert.True(primero.EsValido);
        Assert.Equal("41", primero.Datos["edad"]);
        Assert.Equal(NombreValido, primero.Datos["nombre"]);
    }

    [Fact]
    public void Generar_OpcionQueNoEsLaPorDefecto_TieneSuPropioCasoValido()
    {
        var casos = GeneradorCasos.Generar(HistoriaCon(new CampoLista("tipo de cliente", true, ["Persona", "Empresa"])));

        var empresa = Assert.Single(casos, caso => caso.Datos["tipo de cliente"] == "Empresa");
        Assert.Equal(Opcion, empresa.ClaseProbada);
        Assert.True(empresa.EsValido);
    }

    [Fact]
    public void Generar_TextoDeLongitudCero_ElValorLimiteSeTrataComoVacio()
    {
        var casos = GeneradorCasos.Generar(HistoriaCon(new CampoTexto("comentario", false, 0, 10)));

        var vacio = Assert.Single(casos, caso => caso.Tecnica == ValoresLimite && caso.Datos["comentario"] == "");
        Assert.Equal(Vacio, vacio.ClaseProbada);
        Assert.True(vacio.EsValido);
    }

    [Fact]
    public void Generar_ConUnaRegla_AgregaAlFinalSuTablaDeDecisionConCodigosConsecutivos()
    {
        var sinRegla = GeneradorCasos.Generar(HistoriaConClienteYNit());
        var historia = HistoriaConClienteYNit();
        var regla = new ReglaObligatorio("tipo de cliente", "Empresa", "NIT");
        historia.AgregarRegla(regla);

        var casos = GeneradorCasos.Generar(historia);

        var tabla = casos.Skip(sinRegla.Count).ToList();
        Assert.Equal(sinRegla.Select(caso => caso.Codigo), casos.Take(sinRegla.Count).Select(caso => caso.Codigo));
        Assert.Equal(new[]
        {
            ($"CP-{sinRegla.Count + 1:000}", "Persona", "", true),
            ($"CP-{sinRegla.Count + 2:000}", "Persona", NitConValor, true),
            ($"CP-{sinRegla.Count + 3:000}", "Empresa", "", false),
            ($"CP-{sinRegla.Count + 4:000}", "Empresa", NitConValor, true),
        }, tabla.Select(caso => (caso.Codigo, caso.Datos["tipo de cliente"], caso.Datos["NIT"], caso.EsValido)));
        Assert.All(tabla, caso =>
        {
            Assert.Equal(TablaDecision, caso.Tecnica);
            Assert.Equal(regla, caso.Regla);
            Assert.Equal("NIT", caso.CampoProbado?.Nombre);
            Assert.Equal("41", caso.Datos["edad"]);
        });
    }

    [Fact]
    public void Generar_SinReglas_NingunCasoTraeRegla()
    {
        Assert.All(GeneradorCasos.Generar(HistoriaConClienteYNit()), caso => Assert.Null(caso.Regla));
    }

    [Fact]
    public void Generar_HistoriaSinCampos_NoDevuelveCasos()
    {
        Assert.Empty(GeneradorCasos.Generar(new Historia("Registro de usuario")));
    }

    private static Historia HistoriaCon(params Campo[] campos)
    {
        var historia = new Historia("Registro de usuario");
        foreach (var campo in campos)
        {
            historia.AgregarCampo(campo);
        }

        return historia;
    }

    private static Historia HistoriaConEdadYNombre() =>
        HistoriaCon(new CampoEntero("edad", true, 18, 65), new CampoTexto("nombre", true, 3, 50));

    private static Historia HistoriaConClienteYNit() =>
        HistoriaCon(
            new CampoEntero("edad", true, 18, 65),
            new CampoLista("tipo de cliente", true, ["Persona", "Empresa"]),
            new CampoTexto("NIT", false, 9, 10));
}
