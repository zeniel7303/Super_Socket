using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatServer
{
    public class UserManager
    {
        int maxUserCount;
        UInt64 userSequenceNumber = 0;

        Dictionary<string, User> userMap = new Dictionary<string, User>();

        public void Init(int _maxUserCount)
        {
            maxUserCount = _maxUserCount;
        }

        public CSBaseLib.ERROR_CODE AddUser(string _userID, string _sessionID)
        {
            if (IsFullUserCount())
            {
                return CSBaseLib.ERROR_CODE.LOGIN_FULL_USER_COUNT;
            }

            if (userMap.ContainsKey(_sessionID))
            {
                return CSBaseLib.ERROR_CODE.ADD_USER_DUPLICATION;
            }


            ++userSequenceNumber;

            var user = new User();
            user.Set(userSequenceNumber, _sessionID, _userID);
            userMap.Add(_sessionID, user);

            return CSBaseLib.ERROR_CODE.NONE;
        }

        public CSBaseLib.ERROR_CODE RemoveUser(string _sessionID)
        {
            if (userMap.Remove(_sessionID) == false)
            {
                return CSBaseLib.ERROR_CODE.REMOVE_USER_SEARCH_FAILURE_USER_ID;
            }

            return CSBaseLib.ERROR_CODE.NONE;
        }

        public User GetUser(string _sessionID)
        {
            User user = null;
            userMap.TryGetValue(_sessionID, out user);
            return user;
        }

        bool IsFullUserCount()
        {
            return maxUserCount <= userMap.Count();
        }
    }

    public class User
    {
        UInt64 SequenceNumber = 0;
        //SuperSocket의 SessionID
        string SessionID;

        public int RoomNumber { get; private set; } = -1;
        string UserID;

        public void Set(UInt64 _sequence, string _sessionID, string _userID)
        {
            SequenceNumber = _sequence;
            SessionID = _sessionID;
            UserID = _userID;
        }

        public bool IsConfirm(string _netSessionID)
        {
            return SessionID == _netSessionID;
        }

        public string ID()
        {
            return UserID;
        }

        public void EnterRoom(int _roomNumber)
        {
            RoomNumber = _roomNumber;
        }

        public void LeaveRoom()
        {
            RoomNumber = -1;
        }

        public bool IsStateLogin() { return SequenceNumber != 0; }
        public bool IsStateRoom() { return RoomNumber != -1; }
    }
}
