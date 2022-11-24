using System;

namespace ChattingServer
{
    class Program
    {
        static void Main(string[] args)
        {

            var serverOption = ParseCommandLine(args);
            if(serverOption == null)
            {
                return;
            }

            var serverApp = new MainServer();
            serverApp.InitConfig(serverOption);

            serverApp.CreateStartServer();

            MainServer.MainLogger.Info("Press q to shut down the server");

            while (true)
            {
                System.Threading.Thread.Sleep(50);

                if (Console.KeyAvailable)
                {
                    ConsoleKeyInfo key = Console.ReadKey(true);
                    if (key.KeyChar == 'q')
                    {
                        Console.WriteLine("Server Terminate ~~~");
                        serverApp.StopServer();
                        break;
                    }
                }

            }
        }

        static ChattingServerOption ParseCommandLine(string[] args)
        {
            // 중요한건 이 부분
            var result = CommandLine.Parser.Default.ParseArguments<ChattingServerOption>(args) as CommandLine.Parsed<ChattingServerOption>;

            if (result == null)
            {
                Console.WriteLine("Failed Command Line");
                return null;
            }

            return result.Value;
        }
    }
}
