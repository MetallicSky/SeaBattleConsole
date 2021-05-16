using System;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace SeaBattleConsole
{
    internal class Program
    {
        private static TcpClient client;

        private static void Main(string[] args)
        {
            client = new TcpClient();
            client.Connect("93.191.58.52", 420);

            Task.Factory.StartNew(() =>
            {
                NetworkStream serverCennel = client.GetStream();
                int i = 0;
                Byte[] b = new Byte[255];
                while ((i = serverCennel.Read(b, 0, b.Length)) != 0)
                {
                    Console.WriteLine(System.Text.Encoding.Unicode.GetString(b, 0, i));
                }
            });

            NetworkStream stream = client.GetStream();
            while (true)
            {
                Byte[] data = System.Text.Encoding.Unicode.GetBytes(Console.ReadLine());
                stream.Write(data);
            }
            // Board game = new Board();
            // game.start();
        }
    }
}