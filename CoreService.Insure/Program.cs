using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Hosting;

namespace CoreService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        // Additional configuration is required to successfully run gRPC on macOS.
        // For instructions on how to configure Kestrel and gRPC clients on macOS, visit https://go.microsoft.com/fwlink/?linkid=2099682
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseWindowsService() // Enable running as a Windows service
                .UseSystemd() // Enable running as a Systemd service
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
                    AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2Support", true);
                    //
                    webBuilder.UseStartup<Startup>();
                    webBuilder.ConfigureKestrel(kestrel =>
                    {
                        kestrel.Listen(IPAddress.Any, 5001, listenOptions =>
                        {
                            listenOptions.Protocols = HttpProtocols.Http2;
                        });
                    });
                });

        void Createsigned_Pdf()
        {
            string fileName = @"C:\Temp\TestSigned.pdf";
            var Renderer = new IronPdf.ChromePdfRenderer();
            Renderer.RenderHtmlAsPdf("<h1>Html with CSS and Images</h1>").SaveAs(fileName);

            new IronPdf.Signing.PdfSignature(@"C:\Projects\Keys\insurTech.pfx", "123456").SignPdfFile(fileName);
        }
    }
}
