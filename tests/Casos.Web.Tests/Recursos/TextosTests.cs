using System.Collections;
using System.Globalization;
using System.Resources;
using Casos.Core;
using Casos.Core.Modelo;
using Casos.Web.Componentes;
using Casos.Web.Recursos;

namespace Casos.Web.Tests.Recursos;

public class TextosTests() : PruebaEnIdioma("en")
{
    private readonly Historia historia = HistoriaDeRegistro();

    [Fact]
    public void Recursos_ElInglesTieneLasMismasClavesQueElEspanol()
    {
        var recursos = new ResourceManager("Casos.Web.Recursos.Textos", typeof(Textos).Assembly);

        Assert.Equal(ClavesDe(recursos, CultureInfo.InvariantCulture), ClavesDe(recursos, new CultureInfo("en")));
    }

    [Fact]
    public void Escenario_EnIngles_UsaGivenWhenThen()
    {
        var casos = GeneradorCasos.Generar(historia);

        Assert.Equal(
            """
              Scenario: CP-002 Age less than 18
                Given the form "User sign-up"
                When I enter "17" in age
                And I enter a text of 26 characters in name
                And I choose "Individual" in customer type
                Then the system rejects the age value
            """.ReplaceLineEndings("\n"),
            GherkinCasos.Escenario(historia, casos[1]));
    }

    [Fact]
    public void Caracteristica_EnIngles_EmpiezaConElIdiomaIngles()
    {
        var texto = GherkinCasos.Caracteristica(historia, GeneradorCasos.Generar(historia));

        Assert.StartsWith("# language: en\nFeature: User sign-up\n\n  Scenario: CP-001 All values valid", texto);
    }

    [Theory]
    [InlineData("age", PosicionLimite.EnElMinimo, "Age at the minimum")]
    [InlineData("name", PosicionLimite.EncimaDelMaximo, "Name just above the maximum length")]
    public void Descripcion_EnIngles_DiceDondeCaeElLimite(string campo, PosicionLimite posicion, string esperada)
    {
        var caso = GeneradorCasos.Generar(historia).First(caso => caso.CampoProbado?.Nombre == campo && caso.Posicion == posicion);

        Assert.Equal(esperada, TextosCaso.Descripcion(caso));
    }

    [Fact]
    public void LecturaHoja_EnIngles_DaElNombrePorDefectoYLosAvisosEnIngles()
    {
        var sinNombre = new FilaCampo { Minimo = 1, Maximo = 2 };

        var lectura = LecturaHoja.Leer([sinNombre], "");

        Assert.Equal("Untitled story", lectura.Historia.Nombre);
        Assert.Equal("Enter the field name.", lectura.Avisos[sinNombre]);
    }

    private static IEnumerable<string> ClavesDe(ResourceManager recursos, CultureInfo cultura) =>
        recursos.GetResourceSet(cultura, createIfNotExists: true, tryParents: false)!
            .Cast<DictionaryEntry>()
            .Select(entrada => (string)entrada.Key)
            .Order();

    private static Historia HistoriaDeRegistro()
    {
        var historia = new Historia("User sign-up");
        historia.AgregarCampo(new CampoEntero("age", true, 18, 65));
        historia.AgregarCampo(new CampoTexto("name", true, 3, 50));
        historia.AgregarCampo(new CampoLista("customer type", true, ["Individual", "Company"]));
        return historia;
    }
}
