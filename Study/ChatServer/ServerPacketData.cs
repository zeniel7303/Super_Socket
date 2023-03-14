using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MessagePack;

namespace ChatServer
{
    public class ServerPacketData
    {
        public Int16 pakcetSize;
        public string sessionID;
        public Int16 packetID;
        public SByte type;
        public byte[] bodyData;

        public void Assign(string _sessionID, Int16 _packetID, byte[] _packetBodyData)
        {
            sessionID = _sessionID;
            packetID = _packetID;

            if (_packetBodyData.Length > 0)
            {
                bodyData = _packetBodyData;
            }
        }

        public static ServerPacketData MakeNTFInConnectOrDisConnectClientPacket(
            bool isConnect, string sessionID)
        {
            var packet = new ServerPacketData();

            if (isConnect)
            {
               // packet.PacketID = (Int32)CSBaseLib.PACKETID
            }
            else
            {
                //packet.PacketID = (Int32)CSBaseLib.PACKETID
            }

            packet.sessionID = sessionID;

            return packet;
        }
    }
}
