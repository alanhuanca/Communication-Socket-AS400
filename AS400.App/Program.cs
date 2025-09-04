using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
                "ORDER_001,ITEM_A,10",
                "UPDATE_STATUS_001",
                "INQUIRY_ACCT_12345",
                "ORDER_002,ITEM_B,5"
            };

            var responses = await as400Service.SendMultipleTransactionsAsync(transactions);
            Log.Information("--- Resultados de las transacciones ---");
            if (responses.Count()>0)
            {
                for (int i = 0; i < transactions.Count; i++)
                {
                    Log.Information("Transacción enviada: {Transaction}", transactions[i]);
                    Log.Information("Respuesta recibida: {Response}", responses[i] ?? "FALLÓ");
                }
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
                    // Leer la configuración
                    services.Configure<ClientSettings>(context.Configuration.GetSection("AS400ClientSettings"));

                    // Registrar servicios
                    services.AddTransient<IAs400Service, ClienteAS400Seguro>();

                    // Asegurar que el logger esté disponible para las clases
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