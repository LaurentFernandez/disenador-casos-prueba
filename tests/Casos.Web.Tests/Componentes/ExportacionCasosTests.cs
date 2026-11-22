using Casos.Core;
using Casos.Core.Modelo;
using Casos.Web.Componentes;

namespace Casos.Web.Tests.Componentes;

public class ExportacionCasosTests
{
    private const string EncabezadoCsv = "Código;Descripción;Técnica;edad;nombre;tipo de cliente;Resultado esperado";

    private readonly Historia historia;
    private readonly IReadOnlyList<CasoPrueba> casos;

    public ExportacionCasosTests()
    {
        historia = HistoriaCon(new CampoEntero("edad", true, 18, 65), new CampoTexto("nombre", true, 3, 50), Lista("Persona", "Empresa"));
        casos = GeneradorCasos.Generar(historia);
    }

    [Fact]
    public void Csv_DaElArchivoConSuNombreYLaMarcaUtf8()
    {
        var archivo = ExportacionCasos.Csv(historia, casos);

        Assert.Equal("registro-de-usuario.csv", archivo.Nombre);
        Assert.StartsWith("text/csv", archivo.Tipo);
        Assert.StartsWith("﻿", archivo.Contenido);
    }

    [Fact]
    public void Csv_TieneUnaFilaDeEncabezadosYUnaPorCaso()
    {
        var lineas = LineasCsv(ExportacionCasos.Csv(historia, casos));

        Assert.Equal(casos.Count + 1, lineas.Length);
        Assert.Equal(EncabezadoCsv, lineas[0]);
    }

    [Fact]
    public void Csv_FilaDeUnCaso_LlevaElValorCompleto()
    {
        var lineas = LineasCsv(ExportacionCasos.Csv(historia, casos));

        Assert.Equal($"CP-002;Edad menor que 18;Partición de equivalencia;17;{new string('a', 26)};Persona;Rechaza el valor de edad", lineas[2]);
    }

    [Fact]
    public void Csv_ValorConSeparadorOComillas_VaEntreComillas()
    {
        var conSimbolos = HistoriaCon(Lista("Persona; natural", "Empresa \"grande\""));

        var contenido = ExportacionCasos.Csv(conSimbolos, GeneradorCasos.Generar(conSimbolos)).Contenido;

        Assert.Contains(";\"Persona; natural\";", contenido);
        Assert.Contains(";\"Empresa \"\"grande\"\"\";", contenido);
    }

    [Fact]
    public void Csv_TextoQueParecFormula_SeMarcaComoTextoPeroUnNumeroNegativoNo()
    {
        var conFormula = HistoriaCon(Lista("=SUMA(A1)"), new CampoEntero("temperatura", true, -10, -5));

        var contenido = ExportacionCasos.Csv(conFormula, GeneradorCasos.Generar(conFormula)).Contenido;

        Assert.Contains(";'=SUMA(A1);", contenido);
        Assert.Contains(";-11;", contenido);
        Assert.DoesNotContain("'-11", contenido);
    }

    [Fact]
    public void Markdown_TieneLaTablaConUnaFilaPorCasoComoEnPantalla()
    {
        var archivo = ExportacionCasos.Markdown(historia, casos);
        var lineas = archivo.Contenido.Split('\n');

        Assert.Equal("registro-de-usuario.md", archivo.Nombre);
        Assert.Contains("| Código | Descripción | Técnica | edad | nombre | tipo de cliente | Resultado esperado |", lineas);
        Assert.Contains("| CP-002 | Edad menor que 18 | Partición de equivalencia | 17 | aaaa… (26) | Persona | Rechaza el valor de edad |", lineas);
        Assert.Equal(casos.Count, lineas.Count(linea => linea.StartsWith("| CP-")));
    }

    [Fact]
    public void Markdown_BarraOMenorQueEnUnValor_SeEscapan()
    {
        var conSimbolos = HistoriaCon(Lista("A|B", "<b>"));

        var contenido = ExportacionCasos.Markdown(conSimbolos, GeneradorCasos.Generar(conSimbolos)).Contenido;

        Assert.Contains("| A\\|B |", contenido);
        Assert.Contains("| &lt;b> |", contenido);
    }

    [Fact]
    public void Feature_EsElGherkinDeLaHistoria()
    {
        var archivo = ExportacionCasos.Feature(historia, casos);

        Assert.Equal("registro-de-usuario.feature", archivo.Nombre);
        Assert.Equal(GherkinCasos.Caracteristica(historia, casos), archivo.Contenido);
    }

    [Theory]
    [InlineData("Registro de usuario", "registro-de-usuario")]
    [InlineData("Recuperación de contraseña", "recuperacion-de-contrasena")]
    [InlineData("  ¡Hola, mundo!  ", "hola-mundo")]
    [InlineData("¿?", "casos")]
    public void NombreArchivo_QuitaTildesEspaciosYSignos(string historiaEscrita, string esperado)
    {
        Assert.Equal(esperado, ExportacionCasos.NombreArchivo(historiaEscrita));
    }

    private static string[] LineasCsv(ExportacionCasos.Archivo archivo) =>
        archivo.Contenido.TrimStart('﻿').Split("\r\n", StringSplitOptions.RemoveEmptyEntries);

    private static CampoLista Lista(params string[] opciones) => new("tipo de cliente", true, opciones);

    private static Historia HistoriaCon(params Campo[] campos)
    {
        var historia = new Historia("Registro de usuario");
        foreach (var campo in campos)
        {
            historia.AgregarCampo(campo);
        }

        return historia;
    }
}
