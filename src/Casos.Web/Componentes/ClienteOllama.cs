using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Casos.Web.Recursos;

namespace Casos.Web.Componentes;

/// <summary>
/// Pide a Ollama, que corre en el equipo del usuario, los campos que menciona el texto de una historia. La respuesta
/// solo propone filas de la hoja: las valida la hoja como cualquier fila escrita a mano y los casos los sigue
/// generando el motor.
/// </summary>
public sealed class ClienteOllama(HttpClient http, string modelo)
{
    private const string RutaChat = "api/chat";
    private const string SeparadorOpciones = ", ";

    // Probadas con llama3.2: con menos detalle olvida el mínimo de un texto o traduce la historia, y con un ejemplo de
    // rango con números copia esos números.
    private const string Instrucciones =
        "Extraes los campos de un formulario a partir de una historia de usuario. Usa las mismas palabras de la " +
        "historia, en su idioma, sin traducirlas. Para cada campo que la historia menciona da: el nombre, en minúsculas " +
        "y con las palabras separadas por espacios; el tipo (Entero, Texto o Lista), y si es obligatorio. Un campo es " +
        "opcional solo si la historia lo dice, por ejemplo con \"si quiero\" u \"opcional\". En un Entero, mínimo y " +
        "máximo son el rango de valores. En un Texto, mínimo y máximo son la longitud en caracteres que dice la " +
        "historia: el primer número del rango es el mínimo y el segundo, el máximo. En una Lista van las opciones, sin " +
        "mínimo ni máximo.";

    // Ollama obliga al modelo a responder con esta forma, así que la respuesta se puede leer sin adivinar.
    private static readonly object Esquema = new
    {
        type = "object",
        required = new[] { "campos" },
        properties = new
        {
            campos = new
            {
                type = "array",
                items = new
                {
                    type = "object",
                    required = new[] { "nombre", "tipo", "obligatorio" },
                    properties = new
                    {
                        nombre = new { type = "string" },
                        // El modo IA todavía no propone fechas (HU-17): sus instrucciones solo explican estos tres tipos.
                        tipo = new { type = "string", @enum = new[] { nameof(TipoCampo.Entero), nameof(TipoCampo.Texto), nameof(TipoCampo.Lista) } },
                        obligatorio = new { type = "boolean" },
                        minimo = new { type = new[] { "integer", "null" } },
                        maximo = new { type = new[] { "integer", "null" } },
                        opciones = new { type = "array", items = new { type = "string" } },
                    },
                },
            },
        },
    };

    private static readonly JsonSerializerOptions OpcionesJson = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() },
    };

    /// <summary>
    /// Las filas propuestas o, si no se pudo, el aviso que lo explica.
    /// </summary>
    public sealed record Propuesta(IReadOnlyList<FilaCampo> Filas, string? Aviso = null);

    public string Modelo => modelo;

    public async Task<Propuesta> ProponerAsync(string historia)
    {
        try
        {
            using var respuesta = await http.PostAsJsonAsync(RutaChat, Pedido(historia), OpcionesJson);
            if (respuesta.StatusCode == HttpStatusCode.NotFound)
            {
                return Fallida("Propuesta.SinModelo");
            }

            respuesta.EnsureSuccessStatusCode();
            // La respuesta viene de afuera: cualquier parte puede faltar.
            var chat = await respuesta.Content.ReadFromJsonAsync<RespuestaChat>(OpcionesJson);
            var campos = chat?.Message?.Content is { } contenido
                ? JsonSerializer.Deserialize<CamposPropuestos>(contenido, OpcionesJson)?.Campos
                : null;
            // Una fila sin nada escrito la ignoraría la hoja; tampoco se propone.
            var filas = campos?.OfType<CampoPropuesto>().Select(AFila).Where(fila => !fila.EstaVacia).ToList();
            return filas switch
            {
                null => Fallida("Propuesta.Fallida"),
                [] => Fallida("Propuesta.SinCampos"),
                _ => new Propuesta(filas),
            };
        }
        catch (HttpRequestException excepcion) when (excepcion.StatusCode is null)
        {
            // Sin Ollama en marcha nadie responde en su puerto.
            return Fallida("Propuesta.SinOllama");
        }
        catch (Exception excepcion) when (excepcion is HttpRequestException or JsonException or TaskCanceledException)
        {
            return Fallida("Propuesta.Fallida");
        }
    }

    private object Pedido(string historia) => new
    {
        model = modelo,
        stream = false,
        format = Esquema,
        // Con temperatura 0 la misma historia da la misma propuesta.
        options = new { temperature = 0 },
        messages = new[]
        {
            // Sin decirle el idioma, el modelo tiende a pasar al español una historia escrita en inglés.
            new { role = "system", content = $"{Instrucciones} {Textos.De("Propuesta.IdiomaDeLaHistoria")}" },
            new { role = "user", content = historia },
        },
    };

    // Un nombre vacío o un rango que falta no se corrigen aquí: los avisa la hoja, como en una fila escrita a mano.
    // Los guiones bajos sí, porque el modelo a veces escribe los nombres como variables ("medio_de_pago").
    private static FilaCampo AFila(CampoPropuesto campo) => new()
    {
        Nombre = campo.Nombre?.Replace('_', ' ').Trim() ?? "",
        Tipo = campo.Tipo,
        Obligatorio = campo.Obligatorio,
        Minimo = campo.Tipo == TipoCampo.Lista ? null : campo.Minimo,
        Maximo = campo.Tipo == TipoCampo.Lista ? null : campo.Maximo,
        Opciones = campo.Tipo == TipoCampo.Lista ? string.Join(SeparadorOpciones, campo.Opciones ?? []) : "",
    };

    // Los avisos que nombran el modelo lo reciben como argumento; los demás lo ignoran.
    private Propuesta Fallida(string clave) => new([], Textos.De(clave, modelo));

    private sealed record RespuestaChat(MensajeChat? Message);

    private sealed record MensajeChat(string? Content);

    private sealed record CamposPropuestos(List<CampoPropuesto?>? Campos);

    private sealed record CampoPropuesto(string? Nombre, TipoCampo Tipo, bool Obligatorio, int? Minimo, int? Maximo, List<string>? Opciones);
}
