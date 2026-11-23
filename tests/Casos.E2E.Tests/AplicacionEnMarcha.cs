using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;

namespace Casos.E2E.Tests;

/// <summary>
/// Levanta la aplicación con su servidor de desarrollo en un puerto libre, una sola vez para todas las pruebas, y la
/// cierra al terminar. Usa la compilación que ya existe, en la misma configuración que estas pruebas.
/// </summary>
public sealed class AplicacionEnMarcha : IAsyncLifetime
{
    private const string Solucion = "DisenadorCasosPrueba.slnx";
    private static readonly TimeSpan EsperaMaxima = TimeSpan.FromSeconds(90);
    private static readonly TimeSpan PausaEntreIntentos = TimeSpan.FromMilliseconds(500);

    private readonly StringBuilder salida = new();
    private Process? servidor;

    public string Direccion { get; private set; } = "";

    public async Task InitializeAsync()
    {
        Direccion = $"http://127.0.0.1:{PuertoLibre()}/";
        var configuracion = typeof(AplicacionEnMarcha).Assembly.GetCustomAttribute<AssemblyConfigurationAttribute>()!.Configuration;
        var inicio = new ProcessStartInfo("dotnet")
        {
            ArgumentList =
            {
                "run", "--project", Path.Combine(RaizDelRepositorio(), "src", "Casos.Web"),
                "--configuration", configuracion, "--no-build", "--no-launch-profile",
            },
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };
        inicio.Environment["ASPNETCORE_URLS"] = Direccion;
        // Sin el perfil de lanzamiento hay que pedir el entorno de desarrollo, que es el que sirve los archivos compilados.
        inicio.Environment["ASPNETCORE_ENVIRONMENT"] = "Development";

        servidor = Process.Start(inicio)!;
        // La salida se lee siempre: si nadie la vacía, el proceso se bloquea al llenarse el búfer.
        servidor.OutputDataReceived += (_, linea) => Anotar(linea.Data);
        servidor.ErrorDataReceived += (_, linea) => Anotar(linea.Data);
        servidor.BeginOutputReadLine();
        servidor.BeginErrorReadLine();
        await EsperarRespuesta();
    }

    public Task DisposeAsync()
    {
        // dotnet run arranca la aplicación en un proceso hijo, que también hay que cerrar.
        servidor?.Kill(entireProcessTree: true);
        servidor?.Dispose();
        return Task.CompletedTask;
    }

    private async Task EsperarRespuesta()
    {
        using var cliente = new HttpClient();
        var reloj = Stopwatch.StartNew();
        while (reloj.Elapsed < EsperaMaxima)
        {
            if (servidor!.HasExited)
            {
                throw new InvalidOperationException($"La aplicación terminó al arrancar:\n{Salida()}");
            }

            try
            {
                if ((await cliente.GetAsync(Direccion)).IsSuccessStatusCode)
                {
                    return;
                }
            }
            catch (HttpRequestException)
            {
                // Todavía no escucha en el puerto.
            }

            await Task.Delay(PausaEntreIntentos);
        }

        throw new TimeoutException($"La aplicación no respondió en {EsperaMaxima.TotalSeconds} segundos:\n{Salida()}");
    }

    private void Anotar(string? linea)
    {
        lock (salida)
        {
            salida.AppendLine(linea);
        }
    }

    private string Salida()
    {
        lock (salida)
        {
            return salida.ToString();
        }
    }

    private static int PuertoLibre()
    {
        var escucha = new TcpListener(IPAddress.Loopback, 0);
        escucha.Start();
        var puerto = ((IPEndPoint)escucha.LocalEndpoint).Port;
        escucha.Stop();
        return puerto;
    }

    private static string RaizDelRepositorio()
    {
        var carpeta = new DirectoryInfo(AppContext.BaseDirectory);
        while (carpeta is not null && !File.Exists(Path.Combine(carpeta.FullName, Solucion)))
        {
            carpeta = carpeta.Parent;
        }

        return carpeta?.FullName ?? throw new DirectoryNotFoundException($"No se encontró {Solucion} en las carpetas superiores.");
    }
}

/// <summary>
/// Junta las pruebas que comparten la aplicación en marcha; xUnit las ejecuta una tras otra.
/// </summary>
[CollectionDefinition(Nombre)]
public sealed class ColeccionAplicacion : ICollectionFixture<AplicacionEnMarcha>
{
    public const string Nombre = "Aplicación en marcha";
}
