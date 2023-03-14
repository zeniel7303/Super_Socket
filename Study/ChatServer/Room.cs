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
