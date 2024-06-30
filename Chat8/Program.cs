using ChatApp;
using System.Net;

namespace Chat8
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            if (args.Length == 0)
            {
                var server = new Server<IPEndPoint>(new UdpServerMessageSource());
                await server.Start();
            }
            else if (args.Length == 1)
            {
                var client = new Client<IPEndPoint>(args[0], new UdpClientMessageSource());
                await client.Start();
            }
            else
            {
                Console.WriteLine("To start the server, launch the app with no arguments");
                Console.WriteLine("To start the client, launch the app with the client name");
            }
        }
    }
}
