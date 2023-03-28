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
        public int Index { get; private set; }
        public int Number { get; private set; }

        int MaxUserCount = 0;

        List<RoomUser> UserList = new List<RoomUser>();

        public static Func<string, byte[], bool> NetSendFunc;
    
        public void Init(int _index, int _number, int _maxUserCount)
        {
            Index = _index;
            Number = _number;
            MaxUserCount = _maxUserCount;
        }

        public bool AddUser(string _userID, string _netSessionID)
        {
            if(GetUser(_userID) != null)
            {
                return false;
            }

            var roomUser = new RoomUser();
            roomUser.Set(_userID, _netSessionID);
            UserList.Add(roomUser);

            return true;
        }

        public void RemoveUser(string _netSessionID)
        {
            var index = UserList.FindIndex(x => x.netSessionID == _netSessionID);
            UserList.RemoveAt(index);
        }

        public bool RemoveUser(RoomUser _user)
        {
            return UserList.Remove(_user);
        }

        public RoomUser GetUser(string _userID)
        {
            return UserList.Find(x => x.userID == _userID);
        }

        public RoomUser GetUserByNetSessionID(string _netSessionID)
        {
            return UserList.Find(x => x.netSessionID == _netSessionID);
        }

        public int CurrentUserCount()
        {
            return UserList.Count();
        }

        public void NotifyUserList(string _userNetSessionID)
        {
            var packet = new CSBaseLib.PKT_NotifyRoomUserList();
            foreach (var user in UserList)
            {
                packet.UserIDList.Add(user.userID);
            }

            var bodyData = MessagePackSerializer.Serialize(packet);
            var sendPacket = PacketToBytes.Make(PACKETID.NOTIFY_ROOM_USERLIST, bodyData);

            NetSendFunc(_userNetSessionID, sendPacket);
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
            foreach (var user in UserList)
            {
                if (user.netSessionID == excludeNetSessionID)
                {
                    continue;
                }

                NetSendFunc(user.netSessionID, sendPacket);
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
