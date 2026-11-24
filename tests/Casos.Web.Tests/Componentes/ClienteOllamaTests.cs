using System.Net;
using System.Text.Json;
using Casos.Web.Componentes;

namespace Casos.Web.Tests.Componentes;

public class ClienteOllamaTests() : PruebaEnIdioma("es")
{
    private const string HistoriaDeHU14 =
        "Como visitante quiero registrarme con mi edad, entre 18 y 65 años, y mi nombre, de 3 a 50 caracteres";

    private const string RespuestaDeHU14 = """
        {"campos":[
          {"nombre":"edad","tipo":"Entero","obligatorio":true,"minimo":18,"maximo":65},
          {"nombre":" nombre ","tipo":"Texto","obligatorio":true,"minimo":3,"maximo":50}
        ]}
        """;

    private const string AvisoFallida = "No se pudo leer la propuesta de Ollama. Vuelve a intentarlo.";

    [Fact]
    public async Task ProponerAsync_RespuestaDeHU14_DaLasFilasDeEdadYNombre()
    {
        var propuesta = await OllamaSimulado.QueResponde(RespuestaDeHU14).Cliente().ProponerAsync(HistoriaDeHU14);

        Assert.Null(propuesta.Aviso);
        Assert.Equal(
            new[] { ("edad", TipoCampo.Entero, true, (int?)18, (int?)65), ("nombre", TipoCampo.Texto, true, 3, 50) },
            propuesta.Filas.Select(fila => (fila.Nombre, fila.Tipo, fila.Obligatorio, fila.Minimo, fila.Maximo)));
    }

    [Fact]
    public async Task ProponerAsync_Lista_JuntaLasOpcionesConComasYNoLlevaRango()
    {
        const string respuesta = """
            {"campos":[{"nombre":"medio_de_pago","tipo":"Lista","obligatorio":true,"minimo":1,"maximo":3,
                        "opciones":["tarjeta","transferencia","contraentrega"]}]}
            """;

        var fila = Assert.Single((await OllamaSimulado.QueResponde(respuesta).Cliente().ProponerAsync("…")).Filas);

        Assert.Equal("medio de pago", fila.Nombre);
        Assert.Equal("tarjeta, transferencia, contraentrega", fila.Opciones);
        Assert.Null(fila.Minimo);
        Assert.Null(fila.Maximo);
    }

    [Fact]
    public async Task ProponerAsync_Pedido_LlevaElModeloLaHistoriaYElEsquema()
    {
        var ollama = OllamaSimulado.QueResponde(RespuestaDeHU14);

        await ollama.Cliente().ProponerAsync(HistoriaDeHU14);

        var pedido = JsonDocument.Parse(ollama.UltimoCuerpo).RootElement;
        Assert.Equal("http://localhost:11434/api/chat", ollama.UltimoPedido!.RequestUri!.ToString());
        Assert.Equal(OllamaSimulado.Modelo, pedido.GetProperty("model").GetString());
        Assert.False(pedido.GetProperty("stream").GetBoolean());
        Assert.Equal(0, pedido.GetProperty("options").GetProperty("temperature").GetInt32());
        Assert.EndsWith(
            "La historia está en español: escribe los nombres y las opciones en español.",
            pedido.GetProperty("messages")[0].GetProperty("content").GetString());
        Assert.Equal(HistoriaDeHU14, pedido.GetProperty("messages")[1].GetProperty("content").GetString());
        Assert.Equal(
            new[] { "Entero", "Texto", "Lista" },
            pedido.GetProperty("format").GetProperty("properties").GetProperty("campos").GetProperty("items")
                .GetProperty("properties").GetProperty("tipo").GetProperty("enum").EnumerateArray().Select(tipo => tipo.GetString()));
    }

    [Fact]
    public async Task ProponerAsync_SinOllama_AvisaYExplicaComoIniciarloComoEnHU14()
    {
        var propuesta = await OllamaSimulado.Apagado().Cliente().ProponerAsync(HistoriaDeHU14);

        Assert.Empty(propuesta.Filas);
        Assert.Equal(
            "No se encontró Ollama en este equipo. Ábrelo o instálalo desde ollama.com, baja el modelo con «ollama pull llama3.2» y vuelve a intentarlo.",
            propuesta.Aviso);
    }

    [Fact]
    public async Task ProponerAsync_SinElModelo_DiceCualBajar()
    {
        var ollama = new OllamaSimulado(() => new HttpResponseMessage(HttpStatusCode.NotFound));

        var propuesta = await ollama.Cliente().ProponerAsync(HistoriaDeHU14);

        Assert.Equal("Ollama no tiene el modelo «llama3.2». Bájalo con «ollama pull llama3.2» y vuelve a intentarlo.", propuesta.Aviso);
    }

    [Theory]
    [InlineData("no es JSON")]
    [InlineData("{}")]
    [InlineData("""{"campos":[{"nombre":"edad","tipo":"Fecha","obligatorio":true}]}""")]
    public async Task ProponerAsync_RespuestaQueNoSeEntiende_AvisaQueNoSePudoLeer(string contenido)
    {
        var propuesta = await OllamaSimulado.QueResponde(contenido).Cliente().ProponerAsync(HistoriaDeHU14);

        Assert.Empty(propuesta.Filas);
        Assert.Equal(AvisoFallida, propuesta.Aviso);
    }

    [Fact]
    public async Task ProponerAsync_ErrorDeOllama_AvisaQueNoSePudoLeer()
    {
        var ollama = new OllamaSimulado(() => new HttpResponseMessage(HttpStatusCode.InternalServerError));

        Assert.Equal(AvisoFallida, (await ollama.Cliente().ProponerAsync(HistoriaDeHU14)).Aviso);
    }

    [Theory]
    [InlineData("""{"campos":[]}""")]
    [InlineData("""{"campos":[{"nombre":"","tipo":"Entero","obligatorio":true}]}""")]
    public async Task ProponerAsync_SinCampos_AvisaQueNoLosEncontro(string contenido)
    {
        var propuesta = await OllamaSimulado.QueResponde(contenido).Cliente().ProponerAsync("Como visitante quiero entrar");

        Assert.Empty(propuesta.Filas);
        Assert.Equal("Ollama no encontró campos en la historia. Prueba a describirlos con más detalle.", propuesta.Aviso);
    }
}
