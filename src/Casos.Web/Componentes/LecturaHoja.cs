using Casos.Core.Modelo;
using Casos.Web.Recursos;

namespace Casos.Web.Componentes;

/// <summary>
/// La hoja de campos leída: la historia con los campos válidos y el aviso de cada fila con problemas.
/// </summary>
public sealed record LecturaHoja(Historia Historia, IReadOnlyDictionary<FilaCampo, string> Avisos)
{
    // Sin un nombre escrito, la historia igual necesita uno para el Gherkin y la exportación.
    public static string NombrePorDefecto => Textos.De("Historia.SinNombre");

    public static LecturaHoja Leer(IEnumerable<FilaCampo> filas, string nombreHistoria)
    {
        var historia = new Historia(string.IsNullOrWhiteSpace(nombreHistoria) ? NombrePorDefecto : nombreHistoria.Trim());
        var avisos = new Dictionary<FilaCampo, string>();
        foreach (var fila in filas)
        {
            if (!fila.EstaVacia && AvisoDe(fila, historia) is { } aviso)
            {
                avisos[fila] = aviso;
            }
        }

        return new LecturaHoja(historia, avisos);
    }

    // Si la fila es válida agrega su campo a la historia; si no, devuelve el aviso.
    private static string? AvisoDe(FilaCampo fila, Historia historia)
    {
        if (string.IsNullOrWhiteSpace(fila.Nombre))
        {
            return MensajesCampo.FaltaNombre;
        }

        try
        {
            var campo = fila.ACampo();
            if (campo is null)
            {
                return fila.Tipo == TipoCampo.Texto ? MensajesCampo.FaltaLongitud : MensajesCampo.FaltaRango;
            }

            historia.AgregarCampo(campo);
            return null;
        }
        catch (CampoInvalidoException excepcion)
        {
            return MensajesCampo.De(excepcion.Error);
        }
    }
}
