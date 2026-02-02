using System.Reflection;
using DotBoil;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

await builder.AddDotBoil();

var app = builder.Build();

await app.UseDotBoil();

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
