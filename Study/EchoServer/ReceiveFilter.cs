using System;
using System.Collections.Generic;
using System.Text;

using SuperSocket.Common;
using SuperSocket.SocketBase.Protocol;
using SuperSocket.SocketEngine.Protocol;

namespace EchoServer
{
    public class EFBinaryRequestInfo : BinaryRequestInfo
    {
        // 패킷 헤더용 변수
        public Int16 PacketSize { get; private set; }
        public Int16 PacketId { get; private set; }
        public SByte Value { get; private set; }

        public const int HEADERE_SIZE = sizeof(Int16) + sizeof(Int16) + sizeof(SByte); //5


        public EFBinaryRequestInfo(Int16 _packetSize, Int16 _packetId, SByte _value, byte[] body)
            : base(null, body)
        {
            this.PacketSize = _packetSize;
            this.PacketId = _packetId;
            this.Value = _value;
        }
    }

    // SuperSocket 안에서 알아서 Parsing해준다.
    public class ReceiveFilter : FixedHeaderReceiveFilter<EFBinaryRequestInfo>
    {
        public ReceiveFilter() : base (EFBinaryRequestInfo.HEADERE_SIZE)
        { 
        }

        // Header에서 바디 크기를 알아내라.
        // Header 정보는 SuperSocket이 알아서 넣어준다.
        protected override int GetBodyLengthFromHeader(byte[] header, int offset, int length)
        {
            // LittleEndian이 아니면 BigEndian으로 바꾼다.
            // 자주 동작하는 부분은 아니다.
            if (!BitConverter.IsLittleEndian)
                Array.Reverse(header, offset, 2);

            var packetTotalSize = BitConverter.ToInt16(header, offset);
            return packetTotalSize - EFBinaryRequestInfo.HEADERE_SIZE;
        }

        protected override EFBinaryRequestInfo ResolveRequestInfo(ArraySegment<byte> header, byte[] bodyBuffer, int offset, int length)
        {
            // LittleEndian이 아니면 BigEndian으로 바꾼다.
            // 자주 동작하는 부분은 아니다.
            if (!BitConverter.IsLittleEndian)
                Array.Reverse(header.Array, 0, EFBinaryRequestInfo.HEADERE_SIZE);

            return new EFBinaryRequestInfo(BitConverter.ToInt16(header.Array, 0),
                                           BitConverter.ToInt16(header.Array, 0 + 2),
                                           (SByte)header.Array[4],
                                           bodyBuffer.CloneRange(offset, length));
        }
    }
}
