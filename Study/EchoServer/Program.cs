using System;
using CommandLine;

namespace EchoServer
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello SuperSocketLite");

            // 먼저 옵션 Parsing
            var serverOption = ParseCommandLine(args);
            if (serverOption == null)
            {
                //return;
            }

            var server = new MainServer();
            server.InitConfig(serverOption);
            server.CreateServer();

            var IsResult = server.Start();

            if (IsResult)
            {
                MainServer.MainLogger.Info("서버 네트워크 시작");
            }
            else
            {
                Console.WriteLine("[ERROR] 서버 네트워크 시작 실패");
                return;
            }

            MainServer.MainLogger.Info("key를 누르면 종료한다....");
            Console.ReadKey();
        }

        static ServerOption ParseCommandLine(string[] args)
        {
            // 중요한건 이 부분
            // bat에 있는 옵션을 Parsing해 주는 부분
            var result = CommandLine.Parser.Default.ParseArguments<ServerOption>(args) as CommandLine.Parsed<ServerOption>;

            if (result == null)
            {
                Console.WriteLine("Failed Command Line");
                return null;
            }

            return result.Value;
        }
    }

    public class ServerOption
    {
        [Option("port", Required = true, HelpText = "Server Port Number")]
        public int Port { get; set; }

        [Option("maxConnectionNumber", Required = true, HelpText = "MaxConnectionNumber Count")]
        public int MaxConnectionNumber { get; set; } = 0;

        [Option("name", Required = true, HelpText = "Server Name")]
        public string Name { get; set; }
    }
}
