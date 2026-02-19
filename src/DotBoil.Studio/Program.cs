using DotBoil;

var builder = WebApplication.CreateBuilder(args);

await builder.AddDotBoil();

var app = builder.Build();

await app.UseDotBoil();

app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();

app.Run();