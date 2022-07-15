using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Grpc.Net.Client;
using Grpc.Net.Client.Web;
using Microsoft.AspNetCore.Components;
using Cores.GrpcClient.Authentication;
using Microsoft.AspNetCore.Components.Authorization;
using Admin.Services;
using Insure.Services;
using Resource.Services;
using BlazorApp.Client.Services;
using MudBlazor.Services;
using MudBlazor;
using Blazored.LocalStorage;

namespace BlazorApp.Client
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");

            //grpc Web
            builder.Services.AddBlazoredLocalStorage();
            builder.Services.AddOptions();
            builder.Services.AddAuthorizationCore();
            //Http client
            builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

            //gRpc
            builder.Services.AddMyServices();

            //MudBlazor
            builder.Services.AddMudServices(config =>
            {
                config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomLeft;
                config.SnackbarConfiguration.PreventDuplicates = false;
                config.SnackbarConfiguration.NewestOnTop = true;
                config.SnackbarConfiguration.ShowCloseIcon = true;
                config.SnackbarConfiguration.VisibleStateDuration = 4000;
                config.SnackbarConfiguration.HideTransitionDuration = 500;
                config.SnackbarConfiguration.ShowTransitionDuration = 500;
                config.SnackbarConfiguration.SnackbarVariant = Variant.Filled;
            });
            //
            await builder.Build().RunAsync();
        }
    }

    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMyServices(this IServiceCollection services)
        {
            //Create grpc channel
            var baseUri = services.BuildServiceProvider().GetRequiredService<NavigationManager>().BaseUri;
            var httpClient = new HttpClient(new GrpcWebHandler(GrpcWebMode.GrpcWeb, new HttpClientHandler()));
            var channel = GrpcChannel.ForAddress(baseUri, new GrpcChannelOptions { HttpClient = httpClient });

            // grpcService
            services.AddSingleton(services => new grpcAdminService.grpcAdminServiceClient(channel));
            services.AddSingleton(services => new grpcInsureService.grpcInsureServiceClient(channel));
            services.AddSingleton(services => new grpcResourceService.grpcResourceServiceClient(channel));

            //Authentication
            services.AddScoped<IAuthenticationService, AuthenticationService>();
            services.AddScoped<AuthenticationStateProvider, AuthStateProvider>();

            //Setting master
            services.AddSingleton<SettingService>();
            services.AddSingleton<MasterService>();
            services.AddSingleton<VoucherService>();

            //
            return services;
        }
    }
}
