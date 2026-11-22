using Casos.Core.Modelo;
using Casos.Core.Tecnicas;

namespace Casos.Core.Tests.Tecnicas;

public class ParesTests
{
    private static readonly CampoLista[] CamposDeHU12 =
    [
        new("navegador", true, ["Chrome", "Firefox", "Edge"]),
        new("sistema", true, ["Windows", "macOS"]),
        new("idioma", true, ["español", "inglés"]),
    ];

    [Fact]
    public void Generar_TresCamposDeHU12_DaSeisCombinacionesEnLugarDeDoce()
    {
        var combinaciones = Pares.Generar(CamposDeHU12);

        Assert.Equal(6, combinaciones.Count);
        Assert.Empty(ParejasSinCubrir(CamposDeHU12, combinaciones));
    }

    [Fact]
    public void Generar_CadaCombinacion_TraeUnaOpcionDeCadaCampoEnSuOrden()
    {
        var combinaciones = Pares.Generar(CamposDeHU12);

        Assert.All(combinaciones, combinacion =>
        {
            Assert.Equal(CamposDeHU12.Length, combinacion.Count);
            Assert.All(combinacion.Select((opcion, campo) => (opcion, campo)), par => Assert.Contains(par.opcion, CamposDeHU12[par.campo].Opciones));
        });
    }

    [Fact]
    public void Generar_DosCampos_DaTodasLasCombinaciones()
    {
        CampoLista[] campos = [CamposDeHU12[0], CamposDeHU12[1]];

        Assert.Equal(6, Pares.Generar(campos).Distinct(new ComparadorDeCombinaciones()).Count());
    }

    [Fact]
    public void Generar_CuatroCamposDeTresOpciones_CubreTodasLasParejasConMuchasMenosCombinaciones()
    {
        var campos = Enumerable.Range(1, 4).Select(numero => new CampoLista($"campo {numero}", true, ["a", "b", "c"])).ToArray();

        var combinaciones = Pares.Generar(campos);

        Assert.Empty(ParejasSinCubrir(campos, combinaciones));
        // 9 es el mínimo posible (3 × 3); las combinaciones completas serían 81.
        Assert.InRange(combinaciones.Count, 9, 12);
    }

    [Fact]
    public void Generar_CampoConMasOpcionesAlFinal_IgualCubreTodasLasParejas()
    {
        CampoLista[] campos =
        [
            new("pago", true, ["tarjeta", "efectivo"]),
            new("envío", true, ["normal", "express"]),
            new("país", true, ["Colombia", "México", "Perú", "Chile"]),
        ];

        var combinaciones = Pares.Generar(campos);

        Assert.Empty(ParejasSinCubrir(campos, combinaciones));
        Assert.Equal(8, combinaciones.Count);
    }

    [Fact]
    public void Generar_MismosCampos_DaSiempreLasMismasCombinaciones()
    {
        Assert.Equal(Pares.Generar(CamposDeHU12), Pares.Generar(CamposDeHU12), new ComparadorDeCombinaciones());
    }

    [Fact]
    public void Generar_OpcionRepetida_LaCuentaUnaSolaVez()
    {
        CampoLista[] campos = [new("sistema", true, ["Windows", "Windows", "macOS"]), new("idioma", true, ["español", "inglés"])];

        Assert.Equal(4, Pares.Generar(campos).Count);
    }

    [Fact]
    public void Generar_UnSoloCampo_NoSePuedeCombinar()
    {
        Assert.Throws<ArgumentException>(() => Pares.Generar([CamposDeHU12[0]]));
    }

    // Todas las parejas de opciones de dos campos distintos que no aparecen juntas en ninguna combinación.
    private static IEnumerable<string> ParejasSinCubrir(IReadOnlyList<CampoLista> campos, IReadOnlyList<IReadOnlyList<string>> combinaciones) =>
        from primero in Enumerable.Range(0, campos.Count)
        from segundo in Enumerable.Range(primero + 1, campos.Count - primero - 1)
        from opcionPrimero in campos[primero].Opciones
        from opcionSegundo in campos[segundo].Opciones
        where !combinaciones.Any(combinacion => combinacion[primero] == opcionPrimero && combinacion[segundo] == opcionSegundo)
        select $"{campos[primero].Nombre}={opcionPrimero}, {campos[segundo].Nombre}={opcionSegundo}";

    private sealed class ComparadorDeCombinaciones : IEqualityComparer<IReadOnlyList<string>>
    {
        public bool Equals(IReadOnlyList<string>? x, IReadOnlyList<string>? y) => x!.SequenceEqual(y!);

        public int GetHashCode(IReadOnlyList<string> combinacion) => string.Join('|', combinacion).GetHashCode();
    }
}
