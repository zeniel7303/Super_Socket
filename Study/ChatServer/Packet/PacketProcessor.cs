using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Threading.Tasks.Dataflow;

namespace ChatServer
{
    // 이곳에서 패킷을 처리하고 있음
    class PacketProcessor
    {
        bool isThreadRunning = false;
        System.Threading.Thread processThread = null;

        // receive쪽에서 처리하지 않아도 Post에서 블럭킹 되지 않는다. 
        // BufferBlock<T>(DataflowBlockOptions) 에서 DataflowBlockOptions의 BoundedCapacity로 버퍼 가능 수 지정. BoundedCapacity 보다 크게 쌓이면 블럭킹 된다
        // TPL에 존재한다.
        // 일종의 Queue와 비슷함. 다른 점은 ThreadSafe하게 입출력 가능하다.
        BufferBlock<ServerPacketData> MsgBuffer = new BufferBlock<ServerPacketData>();

        UserManager UserManager = new UserManager();

        Tuple<int, int> RoomNumberRange = new Tuple<int, int>(-1, -1);
        List<Room> RoomList = new List<Room>();

        Dictionary<int, Action<ServerPacketData>> PacketHandlerMap
            = new Dictionary<int, Action<ServerPacketData>>();
        PacketHandler_Common CommonPacketHandler = new PacketHandler_Common();
        PacketHandler_Room RoomPacketHandler = new PacketHandler_Room();

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
            // ThreadSafe
            MsgBuffer.Post(data);
        }


        void RegistPacketHandler(MainServer serverNetwork)
        {
            CommonPacketHandler.Init(serverNetwork, UserManager);
            CommonPacketHandler.RegistPacketHandler(PacketHandlerMap);

            RoomPacketHandler.Init(serverNetwork, UserManager);
            RoomPacketHandler.SetRooomList(RoomList);
            RoomPacketHandler.RegistPacketHandler(PacketHandlerMap);
        }

        void Process()
        {
            while(isThreadRunning)
            {
                try
                {
                    // Receive했는데 데이터가 없으면 정지해서 대기함.
                    // 데이터 생기면 다시 깨어남.
                    // ThreadSafe
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
