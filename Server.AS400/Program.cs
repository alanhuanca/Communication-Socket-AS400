using System;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        const int port = 9000;
        var server = new SocketServerSimulator(port);

        try
        {
            server.Start();

            Console.WriteLine("Simulador de servidor iniciado. Ahora puedes ejecutar tu cliente y conectarlo a localhost:9000");
            Console.WriteLine("Presiona Enter para detener el servidor...");
            Console.ReadLine();
        }
        finally
        {
            server.Stop();
        }
    }
}
