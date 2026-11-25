using System.Text.Json;
using Casos.Web.Componentes;

namespace Casos.Web.Tests.Componentes;

public class ArchivoHistoriaTests() : PruebaEnIdioma("es")
{
    private static HistoriaGuardada RegistroDeCliente() => new(
        "Registro de cliente",
        [
            new() { Nombre = "tipo de cliente", Tipo = TipoCampo.Lista, Opciones = "Persona, Empresa" },
            new() { Nombre = "NIT", Tipo = TipoCampo.Texto, Obligatorio = false, Minimo = 9, Maximo = 10 },
            new() { Nombre = "canal", Tipo = TipoCampo.Lista, Opciones = "Web, Tienda" },
        ],
        [new() { CampoCondicion = "tipo de cliente", Opcion = "Empresa", CampoObligatorio = "NIT" }],
        PorPares: true,
        CambiosPropios: true);

    [Fact]
    public void Crear_LlevaElNombreDeLaHistoriaYLaMarcaDeFormatoComoEnHU16()
    {
        var archivo = ArchivoHistoria.Crear(RegistroDeCliente());

        var raiz = JsonDocument.Parse(archivo.Contenido).RootElement;
        Assert.Equal("registro-de-cliente.json", archivo.Nombre);
        Assert.StartsWith("application/json", archivo.Tipo);
        Assert.Equal(ArchivoHistoria.MarcaDeFormato, raiz.GetProperty("formato").GetString());
        Assert.Equal(ArchivoHistoria.VersionActual, raiz.GetProperty("version").GetInt32());
    }

    [Fact]
    public void Crear_HistoriaSinNombre_UsaElNombrePorDefecto()
    {
        Assert.Equal("historia-sin-nombre.json", ArchivoHistoria.Crear(RegistroDeCliente() with { Nombre = " " }).Nombre);
    }

    [Fact]
    public void CrearYLeer_DevuelvenLaMismaHistoriaConSusCamposSuReglaYLaCasilla()
    {
        var original = RegistroDeCliente();

        var importada = ArchivoHistoria.Leer(ArchivoHistoria.Crear(original).Contenido)!;

        Assert.Equal("Registro de cliente", importada.Nombre);
        Assert.Equal(DatosDe(original.Filas), DatosDe(importada.Filas));
        var regla = Assert.Single(importada.Reglas);
        Assert.Equal(("tipo de cliente", "Empresa", "NIT"), (regla.CampoCondicion, regla.Opcion, regla.CampoObligatorio));
        Assert.True(importada.PorPares);
    }

    [Theory]
    [InlineData("")]
    [InlineData("no es JSON")]
    [InlineData("{}")]
    [InlineData("""{"formato":"otra-herramienta","version":1,"historia":{"nombre":"x","filas":[]}}""")]
    [InlineData("""{"formato":"disenador-casos-prueba/historia","version":2,"historia":{"nombre":"x","filas":[]}}""")]
    [InlineData("""{"formato":"disenador-casos-prueba/historia","version":1,"historia":"x"}""")]
    [InlineData("""{"formato":"disenador-casos-prueba/historia","version":1,"historia":{"filas":[{"tipo":"Fecha"}]}}""")]
    public void Leer_ArchivoQueNoEsUnaHistoriaExportada_DevuelveNullComoEnHU16(string texto)
    {
        Assert.Null(ArchivoHistoria.Leer(texto));
    }

    [Fact]
    public void Leer_LoGuardadoEnElNavegadorSinLaMarca_NoSeTomaPorUnArchivo()
    {
        Assert.Null(ArchivoHistoria.Leer(RegistroDeCliente().AJson()));
    }

    private static IEnumerable<(string, TipoCampo, bool, int?, int?, string)> DatosDe(IEnumerable<FilaCampo> filas) =>
        filas.Select(fila => (fila.Nombre, fila.Tipo, fila.Obligatorio, fila.Minimo, fila.Maximo, fila.Opciones));
}
