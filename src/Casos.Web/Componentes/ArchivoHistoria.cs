using System.Text.Json;

namespace Casos.Web.Componentes;

/// <summary>
/// La definición de una historia como archivo, para guardar varias, llevarlas a otro equipo o compartirlas. Lleva lo
/// mismo que se guarda en el navegador, con una marca que permite reconocer el archivo y rechazar cualquier otro JSON.
/// </summary>
public static class ArchivoHistoria
{
    public const string MarcaDeFormato = "disenador-casos-prueba/historia";
    public const int VersionActual = 1;

    // Una historia real pesa unos pocos KB; el límite evita que un archivo enorme bloquee la página.
    public const long TamanoMaximo = 1024 * 1024;

    private const string Tipo = "application/json;charset=utf-8";

    private static readonly JsonSerializerOptions OpcionesJson = new(JsonSerializerDefaults.Web) { WriteIndented = true };

    /// <summary>
    /// El archivo listo para descargar, con el nombre de la historia: «Registro de cliente» da «registro-de-cliente.json».
    /// </summary>
    public static ExportacionCasos.Archivo Crear(HistoriaGuardada historia)
    {
        using var definicion = JsonDocument.Parse(historia.AJson());
        var contenido = new Contenido(MarcaDeFormato, VersionActual, definicion.RootElement);
        var nombre = string.IsNullOrWhiteSpace(historia.Nombre) ? LecturaHoja.NombrePorDefecto : historia.Nombre;
        return new($"{ExportacionCasos.NombreArchivo(nombre)}.json", Tipo, JsonSerializer.Serialize(contenido, OpcionesJson));
    }

    /// <summary>
    /// La historia del archivo, o null si no lo exportó esta herramienta, es de otra versión o está dañado.
    /// </summary>
    public static HistoriaGuardada? Leer(string texto)
    {
        try
        {
            var contenido = JsonSerializer.Deserialize<Contenido>(texto, OpcionesJson);
            return contenido is { Formato: MarcaDeFormato, Version: VersionActual, Historia.ValueKind: JsonValueKind.Object }
                ? HistoriaGuardada.DesdeJson(contenido.Historia.GetRawText())
                : null;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private sealed record Contenido(string? Formato, int Version, JsonElement Historia);
}
