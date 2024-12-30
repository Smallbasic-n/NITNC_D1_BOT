using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Radzen;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddAuthenticationStateDeserialization();
builder.Services.AddRadzenComponents();
builder.Services.AddRadzenCookieThemeService();
builder.Services.AddRadzenQueryStringThemeService();

await builder.Build().RunAsync();