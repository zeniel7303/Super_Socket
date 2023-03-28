using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CSBaseLib;
using MessagePack;

namespace ChatServer
{
    public class ServerPacketData
    {
        public Int16 PacketSize;
        public string SessionID;
        public Int16 PacketID;
        public SByte Type;
        public byte[] BodyData;

        public void Assign(string _sessionID, Int16 _packetID, byte[] _packetBodyData)
        {
            SessionID = _sessionID;
            PacketID = _packetID;

            if (_packetBodyData.Length > 0)
            {
                BodyData = _packetBodyData;
            }
        }

        public static ServerPacketData NotifyConnectOrDisConnectClientPacket(
            bool isConnect, string sessionID)
        {
            var packet = new ServerPacketData();

            if (isConnect)
            {
                packet.PacketID = (Int32)PACKETID.NOTIFY_CONNECT;
            }
            else
            {
                packet.PacketID = (Int32)PACKETID.NOTIFY_DISCONNECT;
            }

            packet.SessionID = sessionID;

            return packet;
        }
    }

    [MessagePackObject]
    public class PKTMake_NofityLeaveRoom
    {
        [Key(0)]
        public int RoomNumber;

        [Key(1)]
        public string UserID;
    }
}
