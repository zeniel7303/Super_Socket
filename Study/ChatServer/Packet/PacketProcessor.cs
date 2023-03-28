using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Threading.Tasks.Dataflow;

namespace ChatServer
{
    class PacketProcessor
    {
        bool isThreadRunning = false;
        System.Threading.Thread processThread = null;

        //receive쪽에서 처리하지 않아도 Post에서 블럭킹 되지 않는다. 
        //BufferBlock<T>(DataflowBlockOptions) 에서 DataflowBlockOptions의 BoundedCapacity로 버퍼 가능 수 지정. 
        //BoundedCapacity 보다 크게 쌓이면 블럭킹 된다
        BufferBlock<ServerPacketData> MsgBuffer = new BufferBlock<ServerPacketData>();

        UserManager UserManager = new UserManager();

        Tuple<int, int> RoomNumberRange = new Tuple<int, int>(-1, -1);
        List<Room> RoomList = new List<Room>();

        Dictionary<int, Action<ServerPacketData>> PacketHandlerMap
            = new Dictionary<int, Action<ServerPacketData>>();

        public void CreateAndStart(List<Room> _roomList, MainServer _mainServer)
        {
            var MaxUserCount = MainServer.ServerOption.RoomMaxCount
                * MainServer.ServerOption.RoomMaxUserCount;

            UserManager.Init(MaxUserCount);

            RoomList = _roomList;
            var minRoomNum = RoomList[0].Number;
            var maxRoomNum = RoomList[0].Number + RoomList.Count() - 1;
            RoomNumberRange = new Tuple<int, int>(minRoomNum, maxRoomNum);

            RegistPacketHandler(_mainServer);

            isThreadRunning = true;
            processThread = new System.Threading.Thread(this.Process);
            processThread.Start();
        }

        public void Destroy()
        {
            isThreadRunning = false;
            MsgBuffer.Complete();
        }

        public void InsertPacket(ServerPacketData data)
        {
            MsgBuffer.Post(data);
        }


        void RegistPacketHandler(MainServer serverNetwork)
        {
           
        }

        void Process()
        {
            while(isThreadRunning)
            {
                try
                {
                    var packet = MsgBuffer.Receive();

                    if(PacketHandlerMap.ContainsKey(packet.PacketID))
                    {
                        PacketHandlerMap[packet.PacketID](packet);
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("세션 번호 {0}, PacketID {1}, 받은 데이터 크기: {2}", 
                            packet.SessionID, 
                            packet.PacketID, packet.
                            BodyData.Length);
                    }
                }
                catch (Exception ex)
                {
                    isThreadRunning.IfTrue(() => MainServer.MainLogger.Error(ex.ToString()));
                }
            }
        }
    }
}
