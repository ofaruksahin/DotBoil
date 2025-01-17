using System.Reflection;
using DotBoil;

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
    "DotBoil.EFCore",
    "DotBoil.MassTransit"
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
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllers()
    .WithStaticAssets();

app.Run();
