using System;
using System.Collections.Generic;
using System.Text;

using SuperSocket.Common;
using SuperSocket.SocketBase.Protocol;
using SuperSocket.SocketEngine.Protocol;

namespace ChattingServer
{
    public class EFBinaryRequestInfo : BinaryRequestInfo
    {
        // 패킷 헤더용 변수
        public Int16 Size { get; private set; }
        public Int16 PacketID { get; private set; }
        public SByte Type { get; private set; }

        public EFBinaryRequestInfo(Int16 _Size, Int16 _PacketID, SByte _Type, byte[] body)
            : base(null, body)
        {
            this.Size = _Size;
            this.PacketID = _PacketID;
            this.Type = _Type;
        }
    }

    // SuperSocket이 알아서 Parsing해줌
    class ReceiveFilter : FixedHeaderReceiveFilter<EFBinaryRequestInfo>
    {
        public ReceiveFilter() : base(CSBaseLib.PacketDef.PACKET_HEADER_SIZE)
        {

        }

        protected override int GetBodyLengthFromHeader(byte[] header, int offset, int length)
        {
            // 자주 동작할 코드는 아님(.net은 대부분 littleEndian기반)
            if (!BitConverter.IsLittleEndian)
            {
                //LittleEndian이 아니면 reverse해서 바꿔주기
                Array.Reverse(header, offset, CSBaseLib.PacketDef.PACKET_HEADER_SIZE);
            }

            var packetSize = BitConverter.ToInt16(header, offset);
            return packetSize = CSBaseLib.PacketDef.PACKET_HEADER_SIZE;
        }

        protected override EFBinaryRequestInfo ResolveRequestInfo(ArraySegment<byte> header, byte[] bodyBuffer, int offset, int length)
        {
            // 자주 동작할 코드는 아님
            if (!BitConverter.IsLittleEndian)
            {
                //LittleEndian이 아니면 reverse해서 바꿔주기
                Array.Reverse(header.Array, 0, CSBaseLib.PacketDef.PACKET_HEADER_SIZE);
            }

            return new EFBinaryRequestInfo(BitConverter.ToInt16(header.Array, 0),
                                           BitConverter.ToInt16(header.Array, 0 + 2),
                                           (SByte)header.Array[4],
                                           bodyBuffer.CloneRange(offset, length));
        }
    }
}
