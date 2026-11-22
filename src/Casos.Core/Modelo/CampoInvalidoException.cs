namespace Casos.Core.Modelo;

/// <summary>
/// Un campo no cumple sus reglas. <see cref="Error"/> dice cuál; el mensaje de la excepción es solo para desarrolladores.
/// </summary>
public sealed class CampoInvalidoException(ErrorCampo error, string nombreParametro)
    : ArgumentException($"Campo inválido: {error}.", nombreParametro)
{
    public ErrorCampo Error { get; } = error;
}
