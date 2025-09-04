using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using AS400.Core;
using AS400.Infrastructure;
using Microsoft.Extensions.Hosting;

namespace AS400.App
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();
            var serviceProvider = host.Services;

            Log.Information("Iniciando el envío de transacciones múltiples...");

            var as400Service = serviceProvider.GetService<IAs400Service>();
            if (as400Service == null)
            {
                Log.Fatal("No se pudo resolver la dependencia para IAs400Service.");
                return;
            }

            var transactions = new List<string>
            {
                "TRAMA_001,DETALLE_TRAMA,VALORES",
                "TRAMA_002,DETALLE_TRAMA,VALORES",
                "TRAMA_003,DETALLE_TRAMA,VALORES",
                "TRAMA_004,DETALLE_TRAMA,VALORES"
            };

            var responses = await as400Service.SendMultipleTransactionsAsync(transactions);

            Log.Information("--- Resultados de las transacciones ---");
            for (int i = 0; i < transactions.Count; i++)
            {
                Log.Information("Transacción enviada: {Transaction}", transactions[i]);
                Log.Information("Respuesta recibida: {Response}", responses[i] ?? "FALLÓ");
            }
            Log.Information("--- Fin de los resultados ---");

            Log.CloseAndFlush();
        }

        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSerilog((hostingContext, loggerConfiguration) =>
                {
                    loggerConfiguration
                        .ReadFrom.Configuration(hostingContext.Configuration)
                        .Enrich.FromLogContext();
                })
                .ConfigureServices((context, services) =>
                {
                    services.Configure<ClientSettings>(context.Configuration.GetSection("AS400ClientSettings"));
                    services.AddTransient<IAs400Service, ClienteAS400Seguro>();
                    services.AddLogging(builder =>
                    {
                        builder.AddSerilog();
                    });
                })
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config.SetBasePath(Directory.GetCurrentDirectory());
                    config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                });
    }
}
