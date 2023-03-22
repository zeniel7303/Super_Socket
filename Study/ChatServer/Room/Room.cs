using CSBaseLib;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatServer
{
    public class Room
    {
        public int index { get; private set; }
        public int number { get; private set; }

        int maxUserCount = 0;

        List<RoomUser> userList = new List<RoomUser>();

        public static Func<string, byte[], bool> netSendFunc;
    
        public void Init(int _index, int _number, int _maxUserCount)
        {
            index = _index;
            number = _number;
            maxUserCount = _maxUserCount;
        }

        public bool AddUser(string _userID, string _netSessionID)
        {
            if(GetUser(_userID) != null)
            {
                return false;
            }

            var roomUser = new RoomUser();
            roomUser.Set(_userID, _netSessionID);
            userList.Add(roomUser);

            return true;
        }

        public void RemoveUser(string _netSessionID)
        {
            var index = userList.FindIndex(x => x.netSessionID == _netSessionID);
            userList.RemoveAt(index);
        }

        public bool RemoveUser(RoomUser _user)
        {
            return userList.Remove(_user);
        }

        public RoomUser GetUser(string _userID)
        {
            return userList.Find(x => x.userID == _userID);
        }

        public RoomUser GetUserByNetSessionID(string _netSessionID)
        {
            return userList.Find(x => x.netSessionID == _netSessionID);
        }

        public int CurrentUserCount()
        {
            return userList.Count();
        }

        public void NotifyUserList(string _userNetSessionID)
        {
            var packet = new CSBaseLib.PKT_NotifyRoomUserList();
            foreach (var user in userList)
            {
                packet.UserIDList.Add(user.userID);
            }

            var bodyData = MessagePackSerializer.Serialize(packet);
            var sendPacket = PacketToBytes.Make(PACKETID.NOTIFY_ROOM_USERLIST, bodyData);

            netSendFunc(_userNetSessionID, sendPacket);
        }

        public void NofifyEnterRoom(string _newUserNetSessionID, string newUserID)
        {
            var packet = new PKT_NotifyEnterRoom();
            packet.UserID = newUserID;

            var bodyData = MessagePackSerializer.Serialize(packet);
            var sendPacket = PacketToBytes.Make(PACKETID.NOTIFY_ENTER_ROOM, bodyData);

            Broadcast(_newUserNetSessionID, sendPacket);
        }

        public void NotifyLeaveRoom(string _userID)
        {
            if (CurrentUserCount() == 0)
            {
                return;
            }

            var packet = new PKT_NofityLeaveRoom();
            packet.UserID = _userID;

            var bodyData = MessagePackSerializer.Serialize(packet);
            var sendPacket = PacketToBytes.Make(PACKETID.NOTIFY_LEAVE_ROOM, bodyData);

            Broadcast("", sendPacket);
        }

        public void Broadcast(string excludeNetSessionID, byte[] sendPacket)
        {
            foreach (var user in userList)
            {
                if (user.netSessionID == excludeNetSessionID)
                {
                    continue;
                }

                netSendFunc(user.netSessionID, sendPacket);
            }
        }
    }

    public class RoomUser
    {
        public string userID { get; private set; }
        public string netSessionID { get; private set; }

        public void Set(string _userID, string _netSessionID)
        {
            userID = _userID;
            netSessionID = _netSessionID;
        }
    }
}
