using System;
using System.Collections.Generic;
using System.Text;

using SuperSocket.Common;
using SuperSocket.SocketBase.Protocol;
using SuperSocket.SocketEngine.Protocol;

namespace EchoServerEx
{
    public class EFBinaryRequestInfo : BinaryRequestInfo
    {
        // 패킷 헤더용 변수
        public Int16 packetSize { get; private set; }
        public Int16 packetId { get; private set; }
        public SByte value { get; private set; }

        public const int HEADERE_SIZE = 5;


        public EFBinaryRequestInfo(Int16 _packetSize, Int16 _packetId, SByte _value, byte[] body)
            : base(null, body)
        {
            this.packetSize = _packetSize;
            this.packetId = _packetId;
            this.value = _value;
        }
    }
    
    // SuperSocket이 알아서 Parsing해줌
    class ReceiveFilter : FixedHeaderReceiveFilter<EFBinaryRequestInfo>
    {
        public ReceiveFilter() : base(EFBinaryRequestInfo.HEADERE_SIZE)
        {

        }

        protected override int GetBodyLengthFromHeader(byte[] header, int offset, int length)
        {
            // 자주 동작할 코드는 아님(.net은 대부분 littleEndian기반)
            if (!BitConverter.IsLittleEndian)
            {
                //LittleEndian이 아니면 reverse해서 바꿔주기
                Array.Reverse(header, offset, 2);
            }

            var packetSize = BitConverter.ToInt16(header, offset);
            return packetSize = EFBinaryRequestInfo.HEADERE_SIZE;
        }

        protected override EFBinaryRequestInfo ResolveRequestInfo(ArraySegment<byte> header, byte[] bodyBuffer, int offset, int length)
        {
            // 자주 동작할 코드는 아님
            if (!BitConverter.IsLittleEndian)
            {
                //LittleEndian이 아니면 reverse해서 바꿔주기
                Array.Reverse(header.Array, 0, EFBinaryRequestInfo.HEADERE_SIZE);
            }

            return new EFBinaryRequestInfo(BitConverter.ToInt16(header.Array, 0),
                                           BitConverter.ToInt16(header.Array, 0 + 2),
                                           (SByte)header.Array[4],
                                           bodyBuffer.CloneRange(offset, length));
        }
    }
}
