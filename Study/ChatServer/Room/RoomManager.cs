using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatServer
{
    class RoomManager
    {
        List<Room> RoomList = new List<Room>();

        public void CreateRooms()
        {
            var maxRoomCount = MainServer.ServerOption.RoomMaxCount;
            var startNumber = MainServer.ServerOption.RoomStartNumber;
            var MaxUserCount = MainServer.ServerOption.RoomMaxUserCount;

            for(int i = 0; i < maxRoomCount; i++)
            {
                var room = new Room();
                room.Init(i, startNumber + i, MaxUserCount);

                RoomList.Add(room);
            }
        }

        public List<Room> GetRoomList() { return RoomList; }
    }
}
