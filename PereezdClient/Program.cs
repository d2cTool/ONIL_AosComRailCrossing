using PereezdClient.Networking;
using System;

namespace PereezdClient
{
    class Program
    {
        static void Main(string[] args)
        {
            AosTcpClientManager aosTcpClientManager = new AosTcpClientManager("192.168.1.78","7777");
            aosTcpClientManager.AosRequestReceivedEvent += AosTcpClientManager_AosRequestReceivedEvent;
            aosTcpClientManager.StatusChangedEvent += AosTcpClientManager_StatusChangedEvent;

            ConsoleKeyInfo consoleKeyInfo = new ConsoleKeyInfo();
            Console.WriteLine("Help: 1 - Connect, 2 - Disconnect, 3 - Send, 0 - Exit");
            do
            {
                Console.Write("Enter command: ");
                consoleKeyInfo = Console.ReadKey(true);
                Console.WriteLine(consoleKeyInfo.KeyChar);

                switch (consoleKeyInfo.KeyChar)
                {
                    case '1':
                        aosTcpClientManager.Connect();
                        break;
                    case '2':
                        aosTcpClientManager.Disconnect();
                        break;
                    case '3':
                        aosTcpClientManager.AddCommand(new Networking.Protocol.AosCommand()
                            { Arguments = "arg1", Class = "class1", Method = "method1", Delay = 1, ObjName = "obj1" }
                        );
                        break;
                    default:
                        Console.WriteLine("Help: 1 - Connect, 2 - Disconnect, 3 - Send, 0 - Exit");
                        break;
                }

            } while (consoleKeyInfo.KeyChar != '0');
        }

        private static void AosTcpClientManager_StatusChangedEvent(object sender, StatusChangedEventArgs e)
        {
            Console.WriteLine(e.TcpListenerStatus);
        }

        private static void AosTcpClientManager_AosRequestReceivedEvent(object sender, AosRequestEventArgs e)
        {
            Console.WriteLine($"class: {e.AosRequest.Class} method: {e.AosRequest.Method}");
        }
    }
}
