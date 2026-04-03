using System.Reflection;
using DotBoil;

var builder = WebApplication.CreateBuilder(args);

Assembly.Load("DotBoil.Studio.Core");

await builder.AddDotBoil();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
await app.UseDotBoil();

app.Run();