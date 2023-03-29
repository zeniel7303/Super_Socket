using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MessagePack;

using CSBaseLib;

namespace ChatServer
{
    public class PacketHandler_Room : PacketHandler
    {
        List<Room> RoomList = null;
        int StartRoomNumber;

        public void SetRooomList(List<Room> _roomList)
        {
            RoomList = _roomList;
            StartRoomNumber = RoomList[0].Number;
        }

        Room GetRoom(int _roomNumber)
        {
            var index = _roomNumber - StartRoomNumber;

            if (index < 0 || index >= RoomList.Count())
            {
                return null;
            }

            return RoomList[index];
        }

        public void RegistPacketHandler(
            Dictionary<int, Action<ServerPacketData>> _packetHandlerMap)
        {
            _packetHandlerMap.Add((int)PACKETID.REQ_ENTER_ROOM, RequestEnterRoom);
            _packetHandlerMap.Add((int)PACKETID.REQ_LEAVE_ROOM, RequestLeaveRoom);
            // 정상적이지 않은 종료시 방에서 나갈때 보내주는 내부용 패킷
            _packetHandlerMap.Add((int)PACKETID.NOTIFY_LEAVE_ROOM, NotifyLeave);
            _packetHandlerMap.Add((int)PACKETID.REQ_ROOM_CHAT, RequestChat);
        }

        public void RequestEnterRoom(ServerPacketData _data)
        {
            var sessionID = _data.SessionID;
            MainServer.MainLogger.Debug("RequestRoomEnter");

            try
            {
                var user = UserManager.GetUser(sessionID);

                if (user == null || user.IsConfirm(sessionID) == false)
                {
                    ResponseEnterRoom(ERROR_CODE.ROOM_ENTER_INVALID_USER, sessionID);
                    return;
                }

                if (user.IsStateRoom())
                {
                    ResponseEnterRoom(ERROR_CODE.ROOM_ENTER_INVALID_STATE, sessionID);
                    return;
                }

                var reqData = MessagePackSerializer.Deserialize<PKT_ReqEnterRoom>(_data.BodyData);

                var room = GetRoom(reqData.RoomNumber);

                if (room == null)
                {
                    ResponseEnterRoom(ERROR_CODE.ROOM_ENTER_INVALID_ROOM_NUMBER, sessionID);
                    return;
                }

                if (room.AddUser(user.ID(), sessionID) == false)
                {
                    ResponseEnterRoom(ERROR_CODE.ROOM_ENTER_FAIL_ADD_USER, sessionID);
                    return;
                }

                user.EnterRoom(reqData.RoomNumber);

                room.NotifyUserList(sessionID);
                room.NofifyEnterRoom(sessionID, user.ID());

                MainServer.MainLogger.Debug("RequestEnterInternal - Success");
            }
            catch (Exception ex)
            {
                MainServer.MainLogger.Error(ex.ToString());
            }
        }

        void ResponseEnterRoom(ERROR_CODE errorCode, string sessionID)
        {
            var resRoomEnter = new PKT_ResEnterRoom()
            {
                Result = (short)errorCode
            };

            var bodyData = MessagePackSerializer.Serialize(resRoomEnter);
            var sendData = PacketToBytes.Make(PACKETID.RES_ENTER_ROOM, bodyData);

            MainServer.SendData(sessionID, sendData);
        }

        public void RequestLeaveRoom(ServerPacketData _data)
        {
            var sessionID = _data.SessionID;
            MainServer.MainLogger.Debug("로그인 요청 받음");

            try
            {
                var user = UserManager.GetUser(sessionID);
                if (user == null)
                {
                    return;
                }

                if (LeaveRoomUser(sessionID, user.RoomNumber) == false)
                {
                    return;
                }

                user.LeaveRoom();

                ResponseLeaveRoom(sessionID);

                MainServer.MainLogger.Debug("Room RequestLeave - Success");
            }
            catch (Exception ex)
            {
                MainServer.MainLogger.Error(ex.ToString());
            }
        }

        bool LeaveRoomUser(string _sessionID, int _roomNumber)
        {
            MainServer.MainLogger.Debug($"LeaveRoomUser. SessionID:{_sessionID}");

            var room = GetRoom(_roomNumber);
            if (room == null)
            {
                return false;
            }

            var roomUser = room.GetUserByNetSessionID(_sessionID);
            if (roomUser == null)
            {
                return false;
            }

            var userID = roomUser.userID;
            room.RemoveUser(roomUser);

            room.NotifyLeaveRoom(userID);
            return true;
        }

        public void ResponseLeaveRoom(string _sessionID)
        {
            var resRoomLeave = new PKT_ResLeaveRoom()
            {
                Result = (short)ERROR_CODE.NONE
            };

            var bodyData = MessagePackSerializer.Serialize(resRoomLeave);
            var sendData = PacketToBytes.Make(PACKETID.RES_LEAVE_ROOM, bodyData);

            MainServer.SendData(_sessionID, sendData);
        }
        
        public void NotifyLeave(ServerPacketData _packetData)
        {
            var sessionID = _packetData.SessionID;
            MainServer.MainLogger.Debug($"NotifyLeaveInternal. SessionID: {sessionID}");

            var reqData = MessagePackSerializer.Deserialize<PKTMake_NofityLeaveRoom>(_packetData.BodyData);
            LeaveRoomUser(sessionID, reqData.RoomNumber);
        }

        (bool, Room, RoomUser) CheckRoomAndRoomUser(string userNetSessionID)
        {
            var user = UserManager.GetUser(userNetSessionID);
            if (user == null)
            {
                return (false, null, null);
            }

            var roomNumber = user.RoomNumber;
            var room = GetRoom(roomNumber);

            if (room == null)
            {
                return (false, null, null);
            }

            var roomUser = room.GetUserByNetSessionID(userNetSessionID);

            if (roomUser == null)
            {
                return (false, room, null);
            }

            return (true, room, roomUser);
        }

        public void RequestChat(ServerPacketData _data)
        {
            var sessionID = _data.SessionID;
            MainServer.MainLogger.Debug("Room RequestChat");

            try
            {
                var roomObject = CheckRoomAndRoomUser(sessionID);

                if (roomObject.Item1 == false)
                {
                    return;
                }


                var reqData = MessagePackSerializer.Deserialize<PKT_ReqRoomChat>(_data.BodyData);

                var notifyPacket = new PKT_NofityRoomChat()
                {
                    UserID = roomObject.Item3.userID,
                    ChatMessage = reqData.ChatMessage
                };

                var Body = MessagePackSerializer.Serialize(notifyPacket);
                var sendData = PacketToBytes.Make(PACKETID.NOTIFY_ROOM_CHAT, Body);

                roomObject.Item2.Broadcast("", sendData);

                MainServer.MainLogger.Debug("Room RequestChat - Success");
            }
            catch (Exception ex)
            {
                MainServer.MainLogger.Error(ex.ToString());
            }
        }
    }
}
