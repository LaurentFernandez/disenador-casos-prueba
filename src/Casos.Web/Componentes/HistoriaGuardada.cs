using System.Text.Json;
using System.Text.Json.Serialization;

namespace Casos.Web.Componentes;

/// <summary>
/// Lo que el usuario dejó en la página, guardado en el navegador para no perderlo al recargar, cambiar de idioma o
/// cerrar la pestaña.
/// </summary>
public sealed record HistoriaGuardada(string Nombre, List<FilaCampo> Filas, List<FilaRegla> Reglas, bool PorPares, bool CambiosPropios)
{
    // El tipo va por su nombre para que reordenar el enum no cambie lo que ya estaba guardado.
    private static readonly JsonSerializerOptions OpcionesJson = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() },
    };

    public string AJson() => JsonSerializer.Serialize(this, OpcionesJson);

    /// <summary>
    /// Lo guardado, o null si no hay nada o no se puede leer (por ejemplo, si lo dejó una versión anterior).
    /// Completa los textos que falten, para que un dato dañado no rompa la página cada vez que se abre.
    /// </summary>
    public static HistoriaGuardada? DesdeJson(string? json)
    {
        if (string.IsNullOrEmpty(json))
        {
            return null;
        }

        try
        {
            var guardada = JsonSerializer.Deserialize<HistoriaGuardada>(json, OpcionesJson);
            if (guardada?.Filas is null)
            {
                return null;
            }

            var filas = guardada.Filas.OfType<FilaCampo>().ToList();
            foreach (var fila in filas)
            {
                fila.Nombre ??= "";
                fila.Opciones ??= "";
            }

            var reglas = (guardada.Reglas ?? []).OfType<FilaRegla>().ToList();
            foreach (var regla in reglas)
            {
                regla.CampoCondicion ??= "";
                regla.Opcion ??= "";
                regla.CampoObligatorio ??= "";
            }

            return guardada with { Nombre = guardada.Nombre ?? "", Filas = filas, Reglas = reglas };
        }
        catch (JsonException)
        {
            return null;
        }
    }
}
