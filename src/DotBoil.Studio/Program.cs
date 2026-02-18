using DotBoil;

var builder = WebApplication.CreateBuilder(args);

await builder.AddDotBoil();

var app = builder.Build();

await app.UseDotBoil();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();

app.Run();