using Casos.Core;
using Casos.Core.Modelo;
using Casos.Web.Componentes;

namespace Casos.Web.Tests.Componentes;

public class DiagramaRectaTests() : PruebaEnIdioma("es")
{
    [Fact]
    public void Crear_CampoEntero_DibujaUnaBandaPorClase()
    {
        var diagrama = DiagramaDe(new CampoEntero("edad", true, 18, 65));

        Assert.Equal(
            new[] { ("menor que 18", false), ("de 18 a 65", true), ("mayor que 65", false) },
            diagrama.Bandas.Select(banda => (banda.Nombre, banda.Valida)));
    }

    [Fact]
    public void Crear_CampoEntero_MarcaLosValoresDeParticionYLosLimites()
    {
        var diagrama = DiagramaDe(new CampoEntero("edad", true, 18, 65));

        Assert.Equal(new long[] { 17, 41, 66 }, ValoresDe(diagrama, Tecnica.ParticionEquivalencia));
        Assert.Equal(new long[] { 17, 18, 19, 64, 65, 66 }, ValoresDe(diagrama, Tecnica.ValoresLimite));
    }

    [Fact]
    public void Crear_CampoEntero_LosValoresQueNoSonNumerosVanEnFichas()
    {
        var diagrama = DiagramaDe(new CampoEntero("edad", true, 18, 65));

        Assert.Equal(
            new[] { ("18.5", false), ("abc", false), ("vacío", false) },
            diagrama.Fichas.Select(ficha => (ficha.Texto, ficha.Valida)));
    }

    [Fact]
    public void Crear_Marcas_GuardanLosCodigosDeSusCasos()
    {
        var diagrama = DiagramaDe(new CampoEntero("edad", true, 18, 65));

        Assert.Equal(new[] { "CP-001" }, Assert.Single(diagrama.Marcas, marca => marca.Valor == 41).Codigos);
        Assert.Equal(
            new[] { "CP-007" },
            Assert.Single(diagrama.Marcas, marca => marca.Valor == 17 && marca.Tecnica == Tecnica.ValoresLimite).Codigos);
    }

    [Fact]
    public void Crear_Posiciones_JuntanALosVecinosYComprimenLosSaltos()
    {
        var diagrama = DiagramaDe(new CampoEntero("edad", true, 18, 65));
        double Posicion(long valor) => diagrama.Marcas.First(marca => marca.Valor == valor).Posicion;

        Assert.True(Posicion(18) - Posicion(17) < Posicion(41) - Posicion(19));
        Assert.Equal(0.5, Posicion(41), precision: 3);
        Assert.All(diagrama.Marcas, marca => Assert.InRange(marca.Posicion, 0.0, 1.0));
    }

    [Fact]
    public void Crear_CampoTexto_DibujaElVacioYLasLongitudes()
    {
        var diagrama = DiagramaDe(new CampoTexto("nombre", true, 3, 50));

        Assert.Equal(
            new[] { ("vacío", false), ("de 1 a 2", false), ("de 3 a 50", true), ("más de 50", false) },
            diagrama.Bandas.Select(banda => (banda.Nombre, banda.Valida)));
        Assert.Equal(new long[] { 2, 3, 4, 49, 50, 51 }, ValoresDe(diagrama, Tecnica.ValoresLimite));
    }

    [Fact]
    public void Crear_CampoTextoOpcionalConMinimoUno_NoDibujaLaBandaMasCortaYElVacioEsValido()
    {
        var diagrama = DiagramaDe(new CampoTexto("comentario", false, 1, 50));

        Assert.Equal(
            new[] { ("vacío", true), ("de 1 a 50", true), ("más de 50", false) },
            diagrama.Bandas.Select(banda => (banda.Nombre, banda.Valida)));
    }

    [Fact]
    public void Crear_CampoLista_SoloTieneFichas()
    {
        var diagrama = DiagramaDe(new CampoLista("tipo de cliente", true, ["Persona", "Empresa"]));

        Assert.Empty(diagrama.Bandas);
        Assert.Empty(diagrama.Marcas);
        Assert.Equal(
            new[] { ("Persona", true), ("Empresa", true), ("Opción no válida", false), ("vacío", false) },
            diagrama.Fichas.Select(ficha => (ficha.Texto, ficha.Valida)));
    }

    [Fact]
    public void Crear_CasosDeTablaDeDecision_NoSonPuntosNiFichasDeLaRecta()
    {
        var nit = new CampoTexto("NIT", false, 9, 10);
        var historia = new Historia("Registro de cliente");
        historia.AgregarCampo(new CampoLista("tipo de cliente", true, ["Persona", "Empresa"]));
        historia.AgregarCampo(nit);
        historia.AgregarRegla(new ReglaObligatorio("tipo de cliente", "Empresa", "NIT"));
        var casos = GeneradorCasos.Generar(historia);
        var codigosDeLaTabla = casos.Where(caso => caso.Tecnica == Tecnica.TablaDecision).Select(caso => caso.Codigo).ToHashSet();

        var diagrama = DiagramaRecta.Crear(nit, casos);

        Assert.Equal(4, codigosDeLaTabla.Count);
        Assert.DoesNotContain(diagrama.Marcas.SelectMany(marca => marca.Codigos).Concat(diagrama.Fichas.SelectMany(ficha => ficha.Codigos)), codigosDeLaTabla.Contains);
    }

    private static DiagramaRecta DiagramaDe(Campo campo)
    {
        var historia = new Historia("Registro de usuario");
        historia.AgregarCampo(campo);
        return DiagramaRecta.Crear(campo, GeneradorCasos.Generar(historia));
    }

    private static IEnumerable<long> ValoresDe(DiagramaRecta diagrama, Tecnica tecnica) =>
        diagrama.Marcas.Where(marca => marca.Tecnica == tecnica).Select(marca => marca.Valor);
}
