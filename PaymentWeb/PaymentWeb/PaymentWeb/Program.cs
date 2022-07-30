using Server.Common;
using MongoDB.Driver;
using MongoDB.Entities;
using PaymentWeb.Services;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Polly.Extensions.Http;
using Polly;
using System.Net.Http.Headers;

//WebApplication
var builder = WebApplication.CreateBuilder(args);

//builder.UseUrls("http://0.0.0.0:6868");
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(6868); // to listen for incoming http connection on port 5001
    //options.ListenAnyIP(5081, configure => configure.UseHttps()); // to listen for incoming https connection on port 7001
});


// AddRazorPages
builder.Services.AddRazorPages();
//builder.Services.AddRazorPages(options =>
//{
//    options.Conventions.AddPageRoute("/DonateOrder", "/DonateOrder/{InitOrderToken}/{TransactionID}");
//    options.Conventions.AddPageRoute("/CashPayment", "/CashPayment/{InitOrderToken}/{TransactionID}");
//});


//Api
builder.Services.AddControllers();

//HttpContextAccessor
builder.Services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();


//Http client
// Create the retry policy we want
var retryPolicy = HttpPolicyExtensions
                .HandleTransientHttpError() // HttpRequestException, 5XX and 408
                .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(retryAttempt));

builder.Services.AddHttpClient(MyConstant.HttpClient_Common, c =>
{
    //Time out 5s
    c.Timeout = TimeSpan.FromSeconds(5);
    c.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
}
).AddPolicyHandler(retryPolicy);


//Hosted Service
builder.Services.AddHostedService<PaymentWorker>();

//Servies
builder.Services.AddSingleton<eBaoService>();
builder.Services.AddSingleton<BHVService>();
builder.Services.AddSingleton<VnPayService>();
builder.Services.AddSingleton<PaymentService>();
builder.Services.AddSingleton<ReportService>();

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
app.MapControllers();
//
app.Run();
