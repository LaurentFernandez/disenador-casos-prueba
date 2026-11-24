using System.Globalization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.JSInterop;
using Casos.Web;
using Casos.Web.Componentes;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// El modo IA local habla con Ollama en el equipo del usuario; su dirección y su modelo están en wwwroot/appsettings.json.
var ollama = builder.Configuration.GetSection("Ollama");
builder.Services.AddSingleton(new ClienteOllama(new HttpClient { BaseAddress = new Uri(ollama["Direccion"]!) }, ollama["Modelo"]!));

var host = builder.Build();

// El idioma se fija al arrancar: el elegido antes o, la primera vez, el del navegador.
var js = host.Services.GetRequiredService<IJSRuntime>();
await using (var idioma = await js.InvokeAsync<IJSObjectReference>("import", "./js/idioma.js"))
{
    var cultura = new CultureInfo(await idioma.InvokeAsync<string>("idiomaInicial"));
    CultureInfo.DefaultThreadCurrentCulture = cultura;
    CultureInfo.DefaultThreadCurrentUICulture = cultura;
}

await host.RunAsync();
