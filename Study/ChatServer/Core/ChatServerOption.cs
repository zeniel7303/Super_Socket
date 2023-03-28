using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatServer
{
    public class ChatServerOption
    {
        [Option("uniqueID", Required = true, HelpText = "Server UniqueID")]
        public int ChatServerUniqueID { get; set; }

        [Option("name", Required = true, HelpText = "Server Name")]
        public string Name { get; set; }

        [Option("maxConnectionNumber", Required = true, HelpText = "MaxConnectionNumber")]
        public int MaxConnectionNumber { get; set; }

        [Option("port", Required = true, HelpText = "Port")]
        public int Port { get; set; }

        // 패킷 최대 크기
        [Option("maxRequestLength", Required = true, HelpText = "maxRequestLength")]
        public int MaxRequestLength { get; set; }

        // SuperSocket의 ReceiveBuffer
        [Option("receiveBufferSize", Required = true, HelpText = "receiveBufferSize")]
        public int ReceiveBufferSize { get; set; }

        // SuperSocket의 SendBuffer
        [Option("sendBufferSize", Required = true, HelpText = "sendBufferSize")]
        public int SendBufferSize { get; set; }

        [Option("roomMaxCount", Required = true, HelpText = "Max Romm Count")]
        public int RoomMaxCount { get; set; } = 0;

        [Option("roomMaxUserCount", Required = true, HelpText = "RoomMaxUserCount")]
        public int RoomMaxUserCount { get; set; } = 0;

        [Option("roomStartNumber", Required = true, HelpText = "RoomStartNumber")]
        public int RoomStartNumber { get; set; } = 0;

    }
}
