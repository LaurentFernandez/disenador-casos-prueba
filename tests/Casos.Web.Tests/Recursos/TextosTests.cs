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

    [Fact]
    public void Ejemplos_EnIngles_TraenLaHistoriaYLosCamposEnIngles()
    {
        var ejemplos = EjemplosHistoria.Todos();
        var registro = LecturaHoja.Leer(ejemplos[0].Filas, ejemplos[0].Historia);

        Assert.Equal(new[] { "User sign-up", "Password recovery", "Shopping cart", "Compatibility" }, ejemplos.Select(ejemplo => ejemplo.Historia));
        Assert.Empty(registro.Avisos);
        Assert.Equal(new[] { "age", "name", "customer type" }, registro.Historia.Campos.Select(campo => campo.Nombre));
    }

    [Fact]
    public void CasoDeRegla_EnIngles_DescribeLaReglaYUsaSusPasos()
    {
        var conRegla = new Historia("Customer sign-up");
        conRegla.AgregarCampo(new CampoLista("customer type", true, ["Individual", "Company"]));
        conRegla.AgregarCampo(new CampoTexto("tax ID", false, 9, 10));
        conRegla.AgregarRegla(new ReglaObligatorio("customer type", "Company", "tax ID"));
        var companySinTaxId = GeneradorCasos.Generar(conRegla).Single(caso => caso.Tecnica == Tecnica.TablaDecision && !caso.EsValido);

        Assert.Equal("Customer type: Company and tax ID with no value", TextosCaso.Descripcion(companySinTaxId));
        Assert.Equal("Decision table", TextosCaso.NombreLargoTecnica(companySinTaxId.Tecnica));
        Assert.EndsWith("And I leave tax ID empty\n    Then the system rejects the tax ID value", GherkinCasos.Escenario(conRegla, companySinTaxId));
    }

    [Fact]
    public void AvisoDeRegla_EnIngles_NombraElCampoQueNoExiste()
    {
        var regla = new FilaRegla { CampoCondicion = "customer type", Opcion = "Company", CampoObligatorio = "tax ID" };
        var tipo = new FilaCampo { Nombre = "customer type", Tipo = TipoCampo.Lista, Opciones = "Individual, Company" };

        var lectura = LecturaHoja.Leer([tipo], "", [regla]);

        Assert.Equal("The field “tax ID” does not exist.", lectura.AvisosReglas[regla]);
    }

    [Fact]
    public void Pares_EnIngles_DaElAvisoElTotalYElNombreDeLaTecnica()
    {
        var tipo = new FilaCampo { Nombre = "customer type", Tipo = TipoCampo.Lista, Opciones = "Individual, Company" };
        var canal = new FilaCampo { Nombre = "channel", Tipo = TipoCampo.Lista, Opciones = "Web, Store, Phone" };

        var unaLista = LecturaHoja.Leer([tipo], "", porPares: true);
        var dosListas = LecturaHoja.Leer([tipo, canal], "", porPares: true);

        Assert.Equal("At least two list fields are needed to combine in pairs.", unaLista.AvisoPares);
        Assert.Equal(
            "Pairwise combinations: 6 of 6 possible.",
            TextosCaso.TotalPares(dosListas.Historia, GeneradorCasos.Generar(dosListas.Historia)));
        Assert.Equal("Pairwise combination", TextosCaso.NombreLargoTecnica(Tecnica.Pares));
    }

    [Fact]
    public void ValorFueraDeLaLista_EnIngles_EsInvalidOptionEnTodasPartesComoEnHU08()
    {
        var casos = GeneradorCasos.Generar(historia);
        var tipoDeCliente = historia.Campos[2];
        var fueraDeLista = casos.Single(caso => caso.ClaseProbada == TipoClase.FueraDeLista);

        Assert.Equal("Invalid option", TextosCaso.Dato(fueraDeLista, tipoDeCliente));
        Assert.Contains(DiagramaRecta.Crear(tipoDeCliente, casos).Fichas, ficha => ficha is { Texto: "Invalid option", Valida: false });
        Assert.Equal("customer type: Invalid option", TextosCaso.ValorProbado(fueraDeLista));
        Assert.Contains("And I choose \"Invalid option\" in customer type", GherkinCasos.Escenario(historia, fueraDeLista));
        foreach (var archivo in new[] { ExportacionCasos.Csv(historia, casos), ExportacionCasos.Markdown(historia, casos) })
        {
            Assert.Contains("Invalid option", archivo.Contenido);
            Assert.DoesNotContain("Opción no válida", archivo.Contenido);
        }
    }

    [Fact]
    public void ValorFueraDeLaLista_ListaConUnaOpcionLlamadaAsi_LeAgregaUnNumero()
    {
        var revision = new Historia("Review");
        revision.AgregarCampo(new CampoLista("status", true, ["Valid option", "Invalid option"]));
        var fueraDeLista = GeneradorCasos.Generar(revision).Single(caso => caso.ClaseProbada == TipoClase.FueraDeLista);

        Assert.Equal("Invalid option 2", TextosCaso.Dato(fueraDeLista, revision.Campos[0]));
    }

    [Fact]
    public void CampoFecha_EnIngles_DescribeSusCasosYUsaSuPasoGherkin()
    {
        var reserva = new Historia("Booking");
        reserva.AgregarCampo(new CampoFecha("booking date", true, new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31)));
        var casos = GeneradorCasos.Generar(reserva);
        var inexistente = casos.Single(caso => caso.ClaseProbada == TipoClase.FechaInexistente);
        var anterior = casos.First(caso => caso.ClaseProbada == TipoClase.MenorQueMinimo);

        Assert.Equal("Date, required, 2026-01-01 to 2026-12-31", TextosCaso.Reglas(reserva.Campos[0]));
        Assert.Equal("Booking date before 2026-01-01", TextosCaso.Descripcion(anterior));
        Assert.Equal("Booking date that does not exist in the calendar", TextosCaso.Descripcion(inexistente));
        Assert.Equal("Booking date the day after the maximum", TextosCaso.Descripcion(casos[^1]));
        Assert.EndsWith(
            "When I enter \"2026-02-29\" in booking date\n    Then the system rejects the booking date value",
            GherkinCasos.Escenario(reserva, inexistente));
    }

    [Fact]
    public void AvisosDeFecha_EnIngles_DicenQueFaltanLasFechasOQueElRangoEstaAlReves()
    {
        var sinFechas = new FilaCampo { Nombre = "booking date", Tipo = TipoCampo.Fecha };
        var alReves = new FilaCampo { Nombre = "return date", Tipo = TipoCampo.Fecha, FechaMinima = new DateOnly(2026, 2, 1), FechaMaxima = new DateOnly(2026, 1, 1) };

        var lectura = LecturaHoja.Leer([sinFechas, alReves], "");

        Assert.Equal("Choose the minimum and the maximum date.", lectura.Avisos[sinFechas]);
        Assert.Equal("The minimum date cannot be after the maximum date.", lectura.Avisos[alReves]);
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
