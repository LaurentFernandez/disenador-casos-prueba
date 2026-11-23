using AngleSharp.Html.Dom;
using Bunit;
using Casos.Web.Componentes;

namespace Casos.Web.Tests.Componentes;

public class HojaReglasTests() : PruebaEnIdioma("es"), IDisposable
{
    private readonly BunitContext contexto = new();

    [Fact]
    public void CampoObligatorioRenombrado_LaReglaLoSigueMostrandoComoEnHU13()
    {
        List<FilaCampo> filas =
        [
            new() { Nombre = "tipo de cliente", Tipo = TipoCampo.Lista, Opciones = "Persona, Empresa" },
            new() { Nombre = "NIT", Tipo = TipoCampo.Texto, Obligatorio = false, Minimo = 9, Maximo = 10 },
        ];
        List<FilaRegla> reglas = [new() { CampoCondicion = "tipo de cliente", Opcion = "Empresa", CampoObligatorio = "NIT" }];
        var hoja = contexto.Render<HojaReglas>(parametros => parametros
            .Add(componente => componente.Reglas, reglas)
            .Add(componente => componente.Filas, filas)
            .Add(componente => componente.Avisos, new Dictionary<FilaRegla, string>()));

        filas[1].Nombre = "RUT";
        hoja.Render(parametros => parametros.Add(componente => componente.Filas, filas));

        var obligatorio = (IHtmlSelectElement)hoja.FindAll("select")[2];
        Assert.Equal("NIT", obligatorio.Value);
        Assert.Equal(new[] { "Elige…", "NIT", "RUT" }, obligatorio.Options.Select(opcion => opcion.Text));
    }

    public void Dispose() => contexto.Dispose();
}
