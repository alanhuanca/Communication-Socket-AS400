using Server.AS400;
using System.Net;
using System.Net.Sockets;
using System.Text;

/// <summary>
/// Un simulador de servidor de sockets simple para probar clientes TCP.
/// </summary>
public class SocketServerSimulator
{
    private readonly int _port;
    private readonly TcpListener _listener;
    private CancellationTokenSource _cts;

    public SocketServerSimulator(int port)
    {
        _port = port;
        _listener = new TcpListener(IPAddress.Loopback, _port);
    }

    /// <summary>
    /// Inicia el servidor para que comience a escuchar conexiones.
    /// </summary>
    public void Start()
    {
        _cts = new CancellationTokenSource();
        Task.Run(() => ListenForClientsAsync(_cts.Token));
    }

    /// <summary>
    /// Detiene el servidor y cancela el proceso de escucha.
    /// </summary>
    public void Stop()
    {
        _cts?.Cancel();
        _listener.Stop();
        Console.WriteLine("Servidor simulado detenido.");
    }

    private async Task ListenForClientsAsync(CancellationToken token)
    {
        _listener.Start();
        Console.WriteLine($"Simulador de servidor escuchando en el puerto {_port}...");

        while (!token.IsCancellationRequested)
        {
            try
            {
                TcpClient client = await _listener.AcceptTcpClientAsync(token);
                // Manejar la conexión del cliente en un nuevo hilo para no bloquear el bucle de escucha
                _ = Task.Run(() => HandleClientAsync(client), token);
            }
            catch (OperationCanceledException)
            {
                // La operación fue cancelada, salimos del bucle
                break;
            }
            catch (SocketException ex)
            {
                Console.WriteLine($"Error de socket al aceptar conexión: {ex.Message}");
            }
        }
    }

    private async Task HandleClientAsync(TcpClient client)
    {
        try
        {
            using (NetworkStream stream = client.GetStream())
            {
                // Este bucle procesará múltiples transacciones del mismo cliente
                while (client.Connected)
                {
                    byte[] buffer = new byte[256];
                    // Lee de forma no bloqueante para detectar si el cliente se desconecta
                    var readTask = stream.ReadAsync(buffer, 0, buffer.Length);
                    var completedTask = await Task.WhenAny(readTask, Task.Delay(100, _cts.Token));

                    if (completedTask.IsCanceled || !client.Connected)
                    {
                        break;
                    }

                    int bytesRead = await readTask;

                    if (bytesRead == 0) // El cliente se ha desconectado
                    {
                        break;
                    }

                    string receivedData = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                    Console.WriteLine($"Datos recibidos del cliente: '{receivedData}'");

                    string response =  ResponseDataJson.GenerateRandomData().ToString();
                    byte[] responseBytes = Encoding.ASCII.GetBytes(response);

                    await stream.WriteAsync(responseBytes, 0, responseBytes.Length);
                    Console.WriteLine($"Respuesta enviada: '{response}'");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al manejar el cliente: {ex.Message}");
        }
        finally
        {
            client.Close();
        }
    }
}
