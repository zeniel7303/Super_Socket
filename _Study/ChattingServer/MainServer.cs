using System;
using System.Collections.Generic;
using System.Text;

using SuperSocket.SocketBase.Logging;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Protocol;
using SuperSocket.SocketBase.Config;

using CSBaseLib;

namespace ChattingServer
{
    public class MainServer : AppServer<ClientSession, EFBinaryRequestInfo>
    {
        public static ChatServerOption ServerOption;
        public static SuperSocket.SocketBase.Logging.ILog MainLogger;

        IServerConfig m_Config;

        PacketProcessor MainPacketProcessor = new PacketProcessor();
        RoomManager RoomMgr = new RoomManager();

        public MainServer()
            : base(new DefaultReceiveFilterFactory<ReceiveFilter, EFBinaryRequestInfo>())
        {
            NewSessionConnected += new SessionHandler<ClientSession>(OnConnected);
            SessionClosed += new SessionHandler<ClientSession, CloseReason>(OnClosed);
            NewRequestReceived += new RequestHandler<ClientSession, EFBinaryRequestInfo>(OnPacketReceived);
        }

        public void InitConfig(ChattingServerOption option)
        {
            m_Config = new SuperSocket.SocketBase.Config.ServerConfig
            {
                Name = option.Name,
                Ip = "Any",
                Port = option.Port,
                Mode = SocketMode.Tcp,
                MaxConnectionNumber = option.MaxConnectionNumber,
                MaxRequestLength = option.MaxRequestLength,
                ReceiveBufferSize = option.ReceiveBufferSize,
                SendBufferSize = option.SendBufferSize
            };
        }

        public void CreateStartServer()
        {
            try
            {
                bool bResult = Setup(new RootConfig(), m_Config, logFactory: new NLogLogFactory());

                if (bResult == false)
                {
                    Console.WriteLine("[ERROR] 서버 네트워크 설정 실패");
                    return;
                }
                else
                {
                    MainLogger = base.Logger;
                    MainLogger.Info("서버 초기화 성공");
                }

                CreateComponent();

                Start();

                MainLogger.Info("서버 생성 성공");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] 서버 생성 실패: {ex.ToString()}");
            }
        }

        public void StopServer()
        {
            Stop();

            MainPacketProcessor.Destory();
        }

        public ERROR_CODE CreateComponent()
        {
            Room.NetSendFunc = this.SendData;
            RoomMgr.CreateRooms();

            MainPacketProcessor = new PacketProcessor();
            MainPacketProcessor.CreateAndStart(RoomMgr.GetRoomsList(), this);

            MainLogger.Info("CreateComponent - Success");
            return ERROR_CODE.NONE;
        }

        public bool SendData(string sessionID, byte[] sendData)
        {
            var session = GetSessionByID(sessionID);

            try
            {
                if (session == null)
                {
                    return false;
                }

                session.Send(sendData, 0, sendData.Length);
            }
            catch (Exception ex)
            {
                // TimeoutException 예외가 발생할 수 있다
                MainServer.MainLogger.Error($"{ex.ToString()},  {ex.StackTrace}");

                session.SendEndWhenSendingTimeOut();
                session.Close();
            }
            return true;
        }

        public void Distribute(ServerPacketData requestPacket)
        {
            MainPacketProcessor.InsertPacket(requestPacket);
        }

        void OnConnected(ClientSession session)
        {
            MainLogger.Info($"세션 번호 {session.SessionID} 접속");
        }

        void OnClosed(ClientSession session, CloseReason reason)
        {
            MainLogger.Info($"세션 번호 {session.SessionID} 접속해제: {reason.ToString()}");
        }

        void OnPacketReceived(ClientSession session, EFBinaryRequestInfo reqInfo)
        {
            MainLogger.Debug(string.Format("세션 번호 {0} 받은 데이터 크기: {1}, ThreadId: {2}", 
                session.SessionID, reqInfo.Body.Length, System.Threading.Thread.CurrentThread.ManagedThreadId));
        }
    }
}
