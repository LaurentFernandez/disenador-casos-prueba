using Casos.Core.Modelo;

namespace Casos.Web.Componentes;

/// <summary>
/// La hoja de campos leída: la historia con los campos válidos y el aviso de cada fila con problemas.
/// </summary>
public sealed record LecturaHoja(Historia Historia, IReadOnlyDictionary<FilaCampo, string> Avisos)
{
    // shortcut: la historia todavía no tiene nombre editable; agregarlo cuando el Gherkin o la exportación lo necesiten.
    private const string NombreHistoria = "Historia sin nombre";

    public static LecturaHoja Leer(IEnumerable<FilaCampo> filas)
    {
        var historia = new Historia(NombreHistoria);
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
