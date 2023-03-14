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
        BufferBlock<ServerPacketData> msgBuffer = new BufferBlock<ServerPacketData>();

        UserManager userManager = new UserManager();

        Tuple<int, int> roomNumberRange = new Tuple<int, int>(-1, -1);
        List<Room> roomList = new List<Room>();

        Dictionary<int, Action<ServerPacketData>> packetHandlerMap
            = new Dictionary<int, Action<ServerPacketData>>();

        public void CreateAndStart(List<Room> _roomList, MainServer _mainServer)
        {
            var maxUserCount = MainServer.serverOption.RoomMaxCount
                * MainServer.serverOption.RoomMaxUserCount;

            userManager.Init(maxUserCount);

            roomList = _roomList;
            var minRoomNum = roomList[0].number;
            var maxRoomNum = roomList[0].number + roomList.Count() - 1;
            roomNumberRange = new Tuple<int, int>(minRoomNum, maxRoomNum);

            RegistPacketHandler(_mainServer);

            isThreadRunning = true;
            processThread = new System.Threading.Thread(this.Process);
            processThread.Start();
        }

        public void Destroy()
        {
            isThreadRunning = false;
            msgBuffer.Complete();
        }

        public void InsertPacket(ServerPacketData data)
        {
            msgBuffer.Post(data);
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

                }
                catch (Exception ex)
                {

                }
            }
        }
    }
}
