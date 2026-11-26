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
    public void Generar_PorPares_AgregaAlFinalLasCombinacionesValidasConCodigosConsecutivos()
    {
        var sinPares = GeneradorCasos.Generar(HistoriaDeHU12());
        var historia = HistoriaDeHU12();
        historia.CombinarPorPares();

        var casos = GeneradorCasos.Generar(historia);

        var pares = casos.Skip(sinPares.Count).ToList();
        Assert.Equal(6, pares.Count);
        Assert.Equal(
            Enumerable.Range(sinPares.Count + 1, 6).Select(numero => $"CP-{numero:000}"),
            pares.Select(caso => caso.Codigo));
        Assert.All(pares, caso =>
        {
            Assert.Equal(Pares, caso.Tecnica);
            Assert.True(caso.EsValido);
            Assert.Null(caso.CampoProbado);
            Assert.Equal(new[] { "navegador", "sistema", "idioma" }, caso.CamposCombinados!.Select(campo => campo.Nombre));
            Assert.Equal("41", caso.Datos["edad"]);
        });
        Assert.Equal(6, pares.Select(caso => (caso.Datos["navegador"], caso.Datos["sistema"])).Distinct().Count());
    }

    [Fact]
    public void Generar_SinPedirPares_NoAgregaCombinaciones()
    {
        Assert.DoesNotContain(GeneradorCasos.Generar(HistoriaDeHU12()), caso => caso.Tecnica == Pares);
    }

    [Fact]
    public void Generar_CampoFecha_DevuelveSusClasesYSusLimitesConCodigosConsecutivos()
    {
        var casos = GeneradorCasos.Generar(HistoriaCon(new CampoFecha("fecha de reserva", true, new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31))));

        Assert.Equal(new (string, Tecnica, string, TipoClase?, PosicionLimite?, bool)[]
        {
            ("CP-001", ParticionEquivalencia, "2026-07-02", null, null, true),
            ("CP-002", ParticionEquivalencia, "2025-12-31", MenorQueMinimo, null, false),
            ("CP-003", ParticionEquivalencia, "2027-01-01", MayorQueMaximo, null, false),
            ("CP-004", ParticionEquivalencia, "2026-02-29", FechaInexistente, null, false),
            ("CP-005", ParticionEquivalencia, "abc", NoEsFecha, null, false),
            ("CP-006", ParticionEquivalencia, "", Vacio, null, false),
            ("CP-007", ValoresLimite, "2025-12-31", MenorQueMinimo, DebajoDelMinimo, false),
            ("CP-008", ValoresLimite, "2026-01-01", DentroDelRango, EnElMinimo, true),
            ("CP-009", ValoresLimite, "2026-01-02", DentroDelRango, EncimaDelMinimo, true),
            ("CP-010", ValoresLimite, "2026-12-30", DentroDelRango, DebajoDelMaximo, true),
            ("CP-011", ValoresLimite, "2026-12-31", DentroDelRango, EnElMaximo, true),
            ("CP-012", ValoresLimite, "2027-01-01", MayorQueMaximo, EncimaDelMaximo, false),
        }, casos.Select(caso => (caso.Codigo, caso.Tecnica, caso.Datos["fecha de reserva"], caso.ClaseProbada, caso.Posicion, caso.EsValido)));
    }

    [Fact]
    public void Generar_ReglaQueHaceObligatoriaUnaFecha_UsaUnaFechaValidaYElVacio()
    {
        var historia = HistoriaCon(
            new CampoLista("tipo de envío", true, ["Normal", "Programado"]),
            new CampoFecha("fecha de entrega", false, new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 31)));
        historia.AgregarRegla(new ReglaObligatorio("tipo de envío", "Programado", "fecha de entrega"));

        var tabla = GeneradorCasos.Generar(historia).Where(caso => caso.Tecnica == TablaDecision).ToList();

        Assert.Equal(
            new[] { ("Programado", "", false), ("Programado", "2026-01-16", true) },
            tabla.Where(caso => caso.Datos["tipo de envío"] == "Programado").Select(caso => (caso.Datos["tipo de envío"], caso.Datos["fecha de entrega"], caso.EsValido)));
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

    private static Historia HistoriaDeHU12() =>
        HistoriaCon(
            new CampoEntero("edad", true, 18, 65),
            new CampoLista("navegador", true, ["Chrome", "Firefox", "Edge"]),
            new CampoLista("sistema", true, ["Windows", "macOS"]),
            new CampoLista("idioma", true, ["español", "inglés"]));

    private static Historia HistoriaConClienteYNit() =>
        HistoriaCon(
            new CampoEntero("edad", true, 18, 65),
            new CampoLista("tipo de cliente", true, ["Persona", "Empresa"]),
            new CampoTexto("NIT", false, 9, 10));
}
