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
        List<Room> roomList = null;
        int startRoomNumber;

        public void SetRooomList(List<Room> _roomList)
        {
            roomList = _roomList;
            startRoomNumber = roomList[0].number;
        }

        Room GetRoom(int _roomNumber)
        {
            var index = _roomNumber - startRoomNumber;

            if (index < 0 || index >= roomList.Count())
            {
                return null;
            }

            return roomList[index];
        }

        public void RegistPacketHandler(
            Dictionary<int, Action<ServerPacketData>> _packetHandlerMap)
        {
            _packetHandlerMap.Add((int)PACKETID.REQ_ROOM_ENTER, RequestEnterRoom);
            _packetHandlerMap.Add((int)PACKETID.REQ_ROOM_LEAVE, RequestLeaveRoom);
            _packetHandlerMap.Add((int)PACKETID.NOTIFY_ROOM_LEAVE, NotifyLeave);
            _packetHandlerMap.Add((int)PACKETID.REQ_ROOM_CHAT, RequestChat);
        }

        public void RequestEnterRoom(ServerPacketData _data)
        {
            var sessionID = _data.sessionID;
            MainServer.mainLogger.Debug("RequestRoomEnter");

            try
            {
                var user = userManager.GetUser(sessionID);

                if (user == null || user.IsConfirm(sessionID) == false)
                {
                    //Response
                    ResponseEnterRoom(ERROR_CODE.ROOM_ENTER_INVALID_USER, sessionID);
                    return;
                }

                if (user.IsStateRoom())
                {
                    ResponseEnterRoom(ERROR_CODE.ROOM_ENTER_INVALID_STATE, sessionID);
                    return;
                }

                var reqData = MessagePackSerializer.Deserialize<PKTReqRoomEnter>(_data.bodyData);

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

                //Notify, Response

                MainServer.mainLogger.Debug("RequestEnterInternal - Success");
            }
            catch (Exception ex)
            {
                MainServer.mainLogger.Error(ex.ToString());
            }
        }

        void ResponseEnterRoom(ERROR_CODE errorCode, string sessionID)
        {
            var resRoomEnter = new PKTResRoomEnter()
            {
                Result = (short)errorCode
            };

            var bodyData = MessagePackSerializer.Serialize(resRoomEnter);
            var sendData = PacketToBytes.Make(PACKETID.RES_ROOM_ENTER, bodyData);

            mainServer.SendData(sessionID, sendData);
        }

        public void RequestLeaveRoom(ServerPacketData _data)
        {

        }

        public void NotifyLeave(ServerPacketData _data)
        {

        }

        public void RequestChat(ServerPacketData _data)
        {

        }
    }
}
