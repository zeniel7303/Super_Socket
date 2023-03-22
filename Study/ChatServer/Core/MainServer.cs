using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Protocol;
using SuperSocket.SocketEngine;

namespace ChatServer
{
    public class MainServer : AppServer<ClientSession, EFBinaryRequestInfo>
    {
        public static ChatServerOption serverOption;
        public static SuperSocket.SocketBase.Logging.ILog mainLogger;

        SuperSocket.SocketBase.Config.IServerConfig m_config;

        PacketProcessor mainPacketProcessor = new PacketProcessor();
        RoomManager roomManager = new RoomManager();

        public MainServer()
            : base(new DefaultReceiveFilterFactory<ReceiveFilter, EFBinaryRequestInfo>())
        {
            NewSessionConnected += new SessionHandler<ClientSession>(OnConnected);
            SessionClosed += new SessionHandler<ClientSession, CloseReason>(OnClosed);
            NewRequestReceived += new RequestHandler<ClientSession, EFBinaryRequestInfo>(OnPacketReceived);
        }

        public void InitConfig(ChatServerOption _option)
        {
            serverOption = _option;

            m_config = new SuperSocket.SocketBase.Config.ServerConfig
            {
                Name = _option.Name,
                Ip = "Any",
                Port = _option.Port,
                Mode = SocketMode.Tcp,
                MaxConnectionNumber = _option.MaxConnectionNumber,
                MaxRequestLength = _option.MaxRequestLength,
                ReceiveBufferSize = _option.ReceiveBufferSize,
                SendBufferSize = _option.SendBufferSize
            };

        }

        public void CreateStartServer()
        {
            try
            {
                bool bResult = Setup(new SuperSocket.SocketBase.Config.RootConfig(), 
                    m_config, logFactory: new SuperSocket.SocketBase.Logging.NLogLogFactory());

                if (bResult == false)
                {
                    Console.WriteLine("[ERROR] 서버 네트워크 설정 실패");
                    return;
                }
                else
                {
                    mainLogger = base.Logger;
                    mainLogger.Info("서버 초기화 성공");
                }


                CreateComponent();

                Start();

                mainLogger.Info("서버 생성 성공");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] 서버 생성 실패: {ex.ToString()}");
            }
        }

        public void StopServer()
        {
            Stop();

            mainPacketProcessor.Destroy();
        }

        public CSBaseLib.ERROR_CODE CreateComponent()
        {
            Room.netSendFunc = this.SendData;
            roomManager.CreateRooms();

            mainPacketProcessor = new PacketProcessor();
            mainPacketProcessor.CreateAndStart(roomManager.GetRoomList(), this);

            mainLogger.Info("CreateComponent - Success");
            return CSBaseLib.ERROR_CODE.NONE;
        }

        public bool SendData(string _sessionID, byte[] _sendData)
        {
            var session = GetSessionByID(_sessionID);

            try
            {
                if (session == null)
                {
                    return false;
                }

                session.Send(_sendData, 0, _sendData.Length);
            }
            catch (Exception ex)
            {
                // TimeoutException 예외가 발생할 수 있다
                MainServer.mainLogger.Error($"{ex.ToString()},  {ex.StackTrace}");

                session.SendEndWhenSendingTimeOut();
                session.Close();
            }
            return true;
        }

        public void Distribute(ServerPacketData _requestPacket)
        {
            mainPacketProcessor.InsertPacket(_requestPacket);
        }

        void OnConnected(ClientSession _session)
        {
            //옵션의 최대 연결 수를 넘으면 SuperSocket이 바로 접속을 짤라버린다. 즉 이 OnConneted 함수가 호출되지 않는다
            mainLogger.Info(string.Format("세션 번호 {0} 접속", _session.SessionID));

            var packet = ServerPacketData.NotifyConnectOrDisConnectClientPacket(true, _session.SessionID);
            Distribute(packet);
        }

        void OnClosed(ClientSession _session, CloseReason _reason)
        {
            mainLogger.Info(string.Format("세션 번호 {0} 접속해제: {1}", _session.SessionID, _reason.ToString()));

            var packet = ServerPacketData.NotifyConnectOrDisConnectClientPacket(false, _session.SessionID);
            Distribute(packet);
        }

        void OnPacketReceived(ClientSession _session, EFBinaryRequestInfo _reqInfo)
        {
            mainLogger.Debug(string.Format("세션 번호 {0} 받은 데이터 크기: {1}, ThreadId: {2}", _session.SessionID, _reqInfo.Body.Length, System.Threading.Thread.CurrentThread.ManagedThreadId));

            var packet = new ServerPacketData();
            packet.sessionID = _session.SessionID;
            packet.packetSize = _reqInfo.packetSize;
            packet.packetID = _reqInfo.packetId;
            packet.type = _reqInfo.type;
            packet.bodyData = _reqInfo.Body;

            Distribute(packet);
        }
    }

    public class ClientSession : AppSession<ClientSession, EFBinaryRequestInfo>
    {
    }
}
