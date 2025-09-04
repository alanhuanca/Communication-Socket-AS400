using AS400.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Security;
using System.Net.Sockets;
using System.Text;

namespace AS400.Infrastructure
{
    public class ClienteAS400Seguro : IAs400Service
    {
        private readonly string _serverIp;
        private readonly int _serverPort;
        private readonly ILogger<ClienteAS400Seguro> _logger;

        private const int MaxRetries = 3;
        private const int BaseDelayMs = 1000;

        public ClienteAS400Seguro(IOptions<ClientSettings> settings, ILogger<ClienteAS400Seguro> logger)
        {
            _serverIp = settings.Value.ServerIp;
            _serverPort = settings.Value.ServerPort;
            _logger = logger;
        }

        public async Task<List<string>> SendMultipleTransactionsAsync(IEnumerable<string> transactions)
        {
            var responses = new List<string>();

            _logger.LogInformation("Conectando a {serverIp}:{serverPort}...", _serverIp, _serverPort);

            using (var client = new TcpClient())
            {
                try
                {
                    await client.ConnectAsync(_serverIp, _serverPort);
                    _logger.LogInformation("Conexión TCP establecida para múltiples transacciones.");

                    //using (var sslStream = new SslStream(client.GetStream(), false, (sender, certificate, chain, errors) => true))
                    //{
                    //    await sslStream.AuthenticateAsClientAsync(_serverIp);
                    //    _logger.LogInformation("Conexión encriptada (TLS) establecida.");

                    //    foreach (var transaction in transactions)
                    //    {
                    //        var response = await SendAndReceiveWithRetryAsync(sslStream, transaction);
                    //        responses.Add(response);
                    //    }
                    //}
                    using (var stream = client.GetStream())
                    {
                        _logger.LogInformation("Conexión TCP establecida (no encriptada).");
                        foreach (var transaction in transactions)
                        {
                            var response = await SendAndReceiveWithRetryAsync(stream, transaction);
                            responses.Add(response);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogCritical(ex, "Fallo crítico en la conexión. La aplicación no puede continuar.");
                }
            }
            return responses;
        }

        //private async Task<string> SendAndReceiveWithRetryAsync(SslStream sslStream, string transactionData)
        private async Task<string> SendAndReceiveWithRetryAsync(NetworkStream sslStream, string transactionData)
        {
            int retries = 0;
            while (retries < MaxRetries)
            {
                try
                {
                    byte[] transactionBytes = Encoding.ASCII.GetBytes(transactionData);
                    await sslStream.WriteAsync(transactionBytes, 0, transactionBytes.Length);

                    var responseBuffer = new byte[256];
                    int bytesRead = await sslStream.ReadAsync(responseBuffer, 0, responseBuffer.Length);

                    _logger.LogInformation("Transacción exitosa: {transactionData}", transactionData);
                    return Encoding.ASCII.GetString(responseBuffer, 0, bytesRead);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error en la transacción '{transactionData}'. Intento {retries} de {maxRetries}", transactionData, retries + 1, MaxRetries);
                    retries++;
                    if (retries < MaxRetries)
                    {
                        int delay = BaseDelayMs * (int)Math.Pow(2, retries - 1);
                        _logger.LogInformation("Esperando {delay}ms antes del próximo reintento...", delay);
                        await Task.Delay(delay);
                    }
                }
            }
            _logger.LogError("Transacción '{transactionData}' falló después de {maxRetries} reintentos.", transactionData, MaxRetries);
            return null;
        }
    }
}
