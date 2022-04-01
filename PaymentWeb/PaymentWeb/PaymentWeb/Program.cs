using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using BlazorApp.Server.Common;
using MongoDB.Driver;
using MongoDB.Entities;
using PaymentWeb.Services;

//WebApplication
var builder = WebApplication.CreateBuilder(args);

// AddRazorPages
builder.Services.AddRazorPages();

//Hosted Service
builder.Services.AddHostedService<PaymentWorker>();


//Mongle DB
bool IsTaskDone = false;
var databaseConfig = builder.Configuration.GetSection(nameof(DatabaseConfig)).Get<DatabaseConfig>();
Action asyncTask = new Action(async () =>
{
    await DB.InitAsync(databaseConfig.DBName, MongoClientSettings.FromConnectionString(databaseConfig.ConnectionString));
    IsTaskDone = true;
});

//Run & wait
var task = new Task(asyncTask);
task.Start();
while (!IsTaskDone)
{
    Thread.Sleep(1000);
}


//Build app
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
//
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.MapRazorPages();
//
app.Run();
