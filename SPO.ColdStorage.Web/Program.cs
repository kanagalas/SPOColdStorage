using Microsoft.EntityFrameworkCore;
using SPO.ColdStorage.Entities;
using SPO.ColdStorage.Entities.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.


var config = new Config(builder.Configuration);

builder.Services.AddSingleton(config);  
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<SPOColdStorageDbContext>(
    options => options.UseSqlServer(config.ConnectionStrings.SQLConnectionString));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();


app.MapControllerRoute(
    name: "default",
    pattern: "{controller}/{action=Index}/{id?}");

app.MapFallbackToFile("index.html"); ;

app.Run();
