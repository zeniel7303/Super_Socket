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
                    Console.WriteLine("[ERROR] 서버 네트워크 설정 실패 ㅠㅠ");
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
                MainServer.mainLogger.Error($"{ex.ToString()},  {ex.StackTrace}");

                session.SendEndWhenSendingTimeOut();
                session.Close();
            }
            return true;
        }

        public void Distribute(ServerPacketData requestPacket)
        {
            mainPacketProcessor.InsertPacket(requestPacket);
        }

        void OnConnected(ClientSession session)
        {
        }

        void OnClosed(ClientSession session, CloseReason reason)
        {
        }

        void OnPacketReceived(ClientSession session, EFBinaryRequestInfo reqInfo)
        {
        }
    }

    public class ClientSession : AppSession<ClientSession, EFBinaryRequestInfo>
    {
    }
}
