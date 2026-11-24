using System.Net;
using System.Net.Http.Json;
using Casos.Web.Componentes;

namespace Casos.Web.Tests.Componentes;

/// <summary>
/// Responde como Ollama sin que Ollama corra: devuelve lo que indique la prueba y guarda el último pedido.
/// </summary>
public sealed class OllamaSimulado(Func<HttpResponseMessage> responder) : HttpMessageHandler
{
    public const string Modelo = "llama3.2";

    public HttpRequestMessage? UltimoPedido { get; private set; }

    public string UltimoCuerpo { get; private set; } = "";

    /// <summary>
    /// Ollama en marcha, cuyo modelo responde con este contenido.
    /// </summary>
    public static OllamaSimulado QueResponde(string contenido) =>
        new(() => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new { message = new { role = "assistant", content = contenido } }),
        });

    /// <summary>
    /// Nadie escucha en el puerto de Ollama.
    /// </summary>
    public static OllamaSimulado Apagado() => new(() => throw new HttpRequestException("Connection refused"));

    public ClienteOllama Cliente() => new(new HttpClient(this) { BaseAddress = new Uri("http://localhost:11434/") }, Modelo);

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage pedido, CancellationToken cancelacion)
    {
        UltimoPedido = pedido;
        UltimoCuerpo = pedido.Content is null ? "" : await pedido.Content.ReadAsStringAsync(cancelacion);
        return responder();
    }
}
