using Casos.Core;
using Casos.Core.Modelo;
using Casos.Web.Componentes;

namespace Casos.Web.Tests.Componentes;

public class GherkinCasosTests
{
    private readonly Historia historia = HistoriaDeRegistro();

    [Fact]
    public void Escenario_CasoInvalido_TieneLosPasosDeHU06()
    {
        var casos = GeneradorCasos.Generar(historia);

        Assert.Equal(
            """
              Escenario: CP-002 Edad menor que 18
                Dado el formulario "Registro de usuario"
                Cuando ingreso "17" en edad
                Y ingreso un texto de 26 caracteres en nombre
                Y elijo "Persona" en tipo de cliente
                Entonces el sistema rechaza el valor de edad
            """.ReplaceLineEndings("\n"),
            GherkinCasos.Escenario(historia, casos[1]));
    }

    [Fact]
    public void Escenario_ValorVacio_DejaElCampoSinValor()
    {
        var nombreVacio = GeneradorCasos.Generar(historia)
            .First(caso => caso.CampoProbado?.Nombre == "nombre" && caso.Datos["nombre"] == "");

        Assert.Contains("\n    Y dejo nombre sin valor\n", GherkinCasos.Escenario(historia, nombreVacio));
    }

    [Fact]
    public void Escenario_CasoValido_TerminaAceptandoLosDatos()
    {
        var todosValidos = GeneradorCasos.Generar(historia)[0];

        Assert.EndsWith("Entonces el sistema acepta los datos", GherkinCasos.Escenario(historia, todosValidos));
    }

    [Fact]
    public void Caracteristica_EmpiezaConElIdiomaYTieneUnEscenarioPorCaso()
    {
        var casos = GeneradorCasos.Generar(historia);

        var texto = GherkinCasos.Caracteristica(historia, casos);

        Assert.StartsWith("# language: es\nCaracterística: Registro de usuario\n\n  Escenario: CP-001", texto);
        Assert.Equal(casos.Count, texto.Split("Escenario:").Length - 1);
    }

    private static Historia HistoriaDeRegistro()
    {
        var historia = new Historia("Registro de usuario");
        historia.AgregarCampo(new CampoEntero("edad", true, 18, 65));
        historia.AgregarCampo(new CampoTexto("nombre", true, 3, 50));
        historia.AgregarCampo(new CampoLista("tipo de cliente", true, ["Persona", "Empresa"]));
        return historia;
    }
}
