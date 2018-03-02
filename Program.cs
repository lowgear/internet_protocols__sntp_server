using System;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace SNTP_server
{
    class Program
    {
        private static void Main(string[] args)
        {
            if (!ServerArguments.TryParse(args, out var arguments, Console.Error))
                return;

            UdpClient socket;
            try
            {
                socket = new UdpClient(123);
            }
            catch (SocketException e)
            {
                Console.Error.WriteLine("Unable to aquire UDP port 123.");
                return;
            }

            Console.WriteLine($"Specified delay is {arguments.SecondsDelay} seconds.");
            Console.WriteLine();

            Task.Run(MakeNtpServer(socket, arguments, Console.Out));

            Console.ReadLine();
        }

        private static Func<Task> MakeNtpServer(UdpClient socket, ServerArguments arguments, TextWriter writer)
        {
            var delay = new TimeSpan(0, 0, 0, arguments.SecondsDelay);
            return async () =>
            {
                while (true)
                {
                    try
                    {
                        var udpReceiveResult = await socket.ReceiveAsync();
                        
                        writer.WriteLine($"{DateTime.Now}  Recieved request from {udpReceiveResult.RemoteEndPoint}.");

                        var frame = new SntpFrame(udpReceiveResult.Buffer);

                        frame.RearrangeForResponse(delay);

                        var responseBytes = frame.ToBytes();

                        writer.WriteLine($"{DateTime.Now}  Started sending response to {udpReceiveResult.RemoteEndPoint}.");
                        await socket.SendAsync(
                            responseBytes,
                            responseBytes.Length,
                            udpReceiveResult.RemoteEndPoint);
                        writer.WriteLine($"{DateTime.Now}  Finished sending response to {udpReceiveResult.RemoteEndPoint}.");
                    }
                    catch (Exception e)
                    {
                        writer.WriteLine(e.Message);
                    }
                }
            };
        }
    }
}