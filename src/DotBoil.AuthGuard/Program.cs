using System.Reflection;
using DotBoil;
using DotBoil.AuthGuard.Application.Infrastructure.Data.ContextOptions;
using DotBoil.AuthGuard.Application.Infrastructure.Data.Contexts;
using DotBoil.AuthGuard.Application.Infrastructure.Services;
using DotBoil.Configuration;
using DotBoil.EFCore;
using DotBoil.Localization;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();

var dotboilAssemblies = new List<string>
{
    "DotBoil",
    "DotBoil.Cors",
    "DotBoil.Localization",
    "DotBoil.Parameter",
    "DotBoil.Caching",
    "DotBoil.Logging",
    "DotBoil.Mapper",
    "DotBoil.Validator",
    "DotBoil.AuthGuard.Application",
    "DotBoil.EFCore"
}.Select(assemblyName => Assembly.Load(assemblyName)).ToArray();

await builder.AddDotBoil(dotboilAssemblies);

var app = builder.Build();

await app.UseDotBoil(dotboilAssemblies);

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllers()
    .WithStaticAssets();

app.Run();