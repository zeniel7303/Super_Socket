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
        public void RegistPacketHandler(
            Dictionary<int, Action<ServerPacketData>> _packetHandlerMap)
        {
            _packetHandlerMap.Add((int)PACKETID.NOTIFY_CONNECT, NotifyConnect);
            _packetHandlerMap.Add((int)PACKETID.NOTIFY_DISCONNECT, NotifyDisConnect);
            _packetHandlerMap.Add((int)PACKETID.REQ_LOGIN, RequestLogin);
        }

        public void NotifyConnect(ServerPacketData _data)
        {
            MainServer.MainLogger.Debug($"Current Connected Session Count: {MainServer.SessionCount}");
        }

        public void NotifyDisConnect(ServerPacketData _data)
        {
            var sessionID = _data.SessionID;
            var user = UserManager.GetUser(sessionID);

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

                    MainServer.Distribute(internalPacket);
                }

                UserManager.RemoveUser(sessionID);
            }

            MainServer.MainLogger.Debug($"Current Connected Session Count: {MainServer.SessionCount}");
        }
        public void RequestLogin(ServerPacketData _data)
        {
            var sessionID = _data.SessionID;
            MainServer.MainLogger.Debug("로그인 요청 받음");

            try
            {
                if (UserManager.GetUser(sessionID) != null)
                {
                    ResponseLogin(ERROR_CODE.LOGIN_ALREADY_WORKING, _data.SessionID);
                    return;
                }

                var reqData = MessagePackSerializer.Deserialize<PKT_ReqLogin>(_data.BodyData);
                var errorCode = UserManager.AddUser(reqData.UserID, sessionID);
                if (errorCode != ERROR_CODE.NONE)
                {
                    ResponseLogin(errorCode, _data.SessionID);

                    if (errorCode == ERROR_CODE.LOGIN_FULL_USER_COUNT)
                    {
                        //NotifyMustCloseToClient(ERROR_CODE.LOGIN_FULL_USER_COUNT, _data.sessionID);
                    }

                    return;
                }

                ResponseLogin(errorCode, _data.SessionID);

                MainServer.MainLogger.Debug("로그인 요청 답변 보냄");

            }
            catch (Exception ex)
            {
                // 패킷 해제에 의해서 로그가 남지 않도록 로그 수준을 Debug로 한다.
                MainServer.MainLogger.Error(ex.ToString());
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

            MainServer.SendData(sessionID, sendData);
        }
    }
}
