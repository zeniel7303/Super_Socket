using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatServer
{
    class RoomManager
    {
        List<Room> roomList = new List<Room>();

        public void CreateRooms()
        {
            var maxRoomCount = MainServer.serverOption.RoomMaxCount;
            var startNumber = MainServer.serverOption.RoomStartNumber;
            var maxUserCount = MainServer.serverOption.RoomMaxUserCount;

            for(int i = 0; i < maxRoomCount; i++)
            {
                var roomNumber = startNumber + i;
                var room = new Room();
                room.Init(i, roomNumber, maxUserCount);

                roomList.Add(room);
            }
        }

        public List<Room> GetRoomList() { return roomList; }
    }
}
