using Bunit;
using Casos.Web.Componentes;
using Microsoft.Extensions.DependencyInjection;

namespace Casos.Web.Tests.Componentes;

public class ProponerCamposTests() : PruebaEnIdioma("es"), IDisposable
{
    private readonly BunitContext contexto = new();
    private IReadOnlyList<FilaCampo>? aceptadas;

    [Fact]
    public void Proponer_MuestraLaPropuestaYAlAceptarlaLaPasaALaHojaComoEnHU14()
    {
        var seccion = Dibujar(OllamaSimulado.QueResponde("""
            {"campos":[
              {"nombre":"edad","tipo":"Entero","obligatorio":true,"minimo":18,"maximo":65},
              {"nombre":"medio de pago","tipo":"Lista","obligatorio":true,"opciones":["tarjeta","transferencia"]}
            ]}
            """));
        Assert.True(Boton(seccion, "Proponer campos").HasAttribute("disabled"));

        Proponer(seccion, "Como cliente quiero…");

        seccion.WaitForAssertion(() => Assert.Equal(
            new[] { "edad: Entero, obligatorio, de 18 a 65", "medio de pago: Lista, obligatorio, con 2 opciones (tarjeta, transferencia)" },
            seccion.FindAll(".propuesta__campos li").Select(campo => campo.TextContent)));
        Assert.Null(aceptadas);

        Boton(seccion, "Aceptar la propuesta").Click();

        Assert.Equal(new[] { "edad", "medio de pago" }, aceptadas!.Select(fila => fila.Nombre));
        Assert.Empty(seccion.FindAll(".propuesta__campos"));
    }

    [Fact]
    public void PropuestaConUnError_MuestraElAvisoDeLaHojaAntesDeAceptarla()
    {
        var seccion = Dibujar(OllamaSimulado.QueResponde("""
            {"campos":[{"nombre":"nombre","tipo":"Texto","obligatorio":true,"minimo":50,"maximo":3}]}
            """));

        Proponer(seccion, "Como visitante quiero…");

        seccion.WaitForAssertion(() =>
        {
            var campo = seccion.Find(".propuesta__campos li");
            Assert.Equal("nombre: La longitud mínima no puede ser mayor que la máxima.", campo.TextContent);
            Assert.Contains("propuesta__campo--aviso", campo.ClassList);
        });
    }

    [Fact]
    public void SinOllama_AvisaEnLaZonaDeEstadoYNoProponeNada()
    {
        var seccion = Dibujar(OllamaSimulado.Apagado());

        Proponer(seccion, "Como visitante quiero…");

        seccion.WaitForAssertion(() => Assert.StartsWith("No se encontró Ollama en este equipo.", seccion.Find("[role=status] .aviso").TextContent));
        Assert.Empty(seccion.FindAll(".propuesta__campos"));
    }

    [Theory]
    [InlineData("http://localhost:5267/", true)]
    [InlineData("http://127.0.0.1:61234/", true)]
    [InlineData("https://laurentfernandez.github.io/disenador-casos-prueba/", false)]
    public void EsLocal_SoloEnElEquipoDelUsuarioComoEnHU14(string direccion, bool esLocal)
    {
        Assert.Equal(esLocal, ProponerCampos.EsLocal(direccion));
    }

    public void Dispose() => contexto.Dispose();

    private IRenderedComponent<ProponerCampos> Dibujar(OllamaSimulado ollama)
    {
        contexto.Services.AddSingleton(ollama.Cliente());
        return contexto.Render<ProponerCampos>(parametros => parametros.Add(
            componente => componente.Aceptar,
            filas =>
            {
                aceptadas = filas;
                return Task.FromResult(true);
            }));
    }

    private static void Proponer(IRenderedComponent<ProponerCampos> seccion, string historia)
    {
        seccion.Find("textarea").Input(historia);
        Boton(seccion, "Proponer campos").Click();
    }

    private static AngleSharp.Dom.IElement Boton(IRenderedComponent<ProponerCampos> seccion, string texto) =>
        seccion.FindAll("button").Single(boton => boton.TextContent == texto);
}
