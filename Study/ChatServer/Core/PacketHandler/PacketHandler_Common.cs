using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MessagePack;

using CSBaseLib;

namespace ChatServer
{
    public class PacketHandler_Common : PacketHandler
    {
        public void RegisterPacketHandler(
            Dictionary<int, Action<ServerPacketData>> _packetHandlerMap)
        {
            _packetHandlerMap.Add((int)PACKETID.NOTIFY_CONNECT, NotifyConnect);
            _packetHandlerMap.Add((int)PACKETID.NOTIFY_DISCONNECT, NotifyDisConnect);
            _packetHandlerMap.Add((int)PACKETID.REQ_LOGIN, RequestLogin);
        }

        public void NotifyConnect(ServerPacketData _data)
        {
            MainServer.mainLogger.Debug($"Current Connected Session Count: {mainServer.SessionCount}");
        }

        public void NotifyDisConnect(ServerPacketData _data)
        {
            var sessionID = _data.sessionID;
            var user = userManager.GetUser(sessionID);

            if (user != null)
            {
                var roomNum = user.RoomNumber;

                if (roomNum != PacketDef.INVALID_ROOM_NUMBER)
                {
                    var packet = new PKTMake_NofityLeaveRoom()
                    {
                        RoomNumber = roomNum,
                        UserID = user.ID(),
                    };

                    var packetBodyData = MessagePackSerializer.Serialize(packet);
                    var internalPacket = new ServerPacketData();
                    internalPacket.Assign(sessionID, (Int16)PACKETID.NOTIFY_LEAVE_ROOM, packetBodyData);

                    mainServer.Distribute(internalPacket);
                }

                userManager.RemoveUser(sessionID);
            }

            MainServer.mainLogger.Debug($"Current Connected Session Count: {mainServer.SessionCount}");
        }
        public void RequestLogin(ServerPacketData _data)
        {
            var sessionID = _data.sessionID;
            MainServer.mainLogger.Debug("로그인 요청 받음");

            try
            {
                if (userManager.GetUser(sessionID) != null)
                {
                    ResponseLogin(ERROR_CODE.LOGIN_ALREADY_WORKING, _data.sessionID);
                    return;
                }

                var reqData = MessagePackSerializer.Deserialize<PKT_ReqLogin>(_data.bodyData);
                var errorCode = userManager.AddUser(reqData.UserID, sessionID);
                if (errorCode != ERROR_CODE.NONE)
                {
                    ResponseLogin(errorCode, _data.sessionID);

                    if (errorCode == ERROR_CODE.LOGIN_FULL_USER_COUNT)
                    {
                        //NotifyMustCloseToClient(ERROR_CODE.LOGIN_FULL_USER_COUNT, _data.sessionID);
                    }

                    return;
                }

                ResponseLogin(errorCode, _data.sessionID);

                MainServer.mainLogger.Debug("로그인 요청 답변 보냄");

            }
            catch (Exception ex)
            {
                // 패킷 해제에 의해서 로그가 남지 않도록 로그 수준을 Debug로 한다.
                MainServer.mainLogger.Error(ex.ToString());
            }
        }

        public void ResponseLogin(ERROR_CODE errorCode, string sessionID)
        {
            var resLogin = new PKT_ResLogin()
            {
                Result = (short)errorCode
            };

            var bodyData = MessagePackSerializer.Serialize(resLogin);
            var sendData = PacketToBytes.Make(PACKETID.RES_LOGIN, bodyData);

            mainServer.SendData(sessionID, sendData);
        }
    }
}
