using CSBaseLib;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TestClient
{
    enum CLIENT_STATE
    {
        NONE = 0,
        CONNECTED = 1,
        LOGIN = 2,
    }

    struct PacketData
    {
        public Int16 DataSize;
        public Int16 PacketID;
        public SByte Type;
        public byte[] BodyData;
    }

    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        CLIENT_STATE ClientState = CLIENT_STATE.NONE;

        SimpleTCPClient Network = new SimpleTCPClient();
        System.Threading.Thread NetworkReadThread = null;
        System.Threading.Thread NetworkSendThread = null;

        bool IsNetworkThreadRunning = false;
        bool IsMainProcessRunning = false;

        PacketBufferManager PacketBuffer = new PacketBufferManager();
        Queue<PacketData> RecvPacketQueue = new Queue<PacketData>();
        Queue<byte[]> SendPacketQueue = new Queue<byte[]>();

        System.Windows.Threading.DispatcherTimer dispatcherUITimer = new System.Windows.Threading.DispatcherTimer();

        public MainWindow()
        {
            InitializeComponent();

            PacketBuffer.Init((8096 * 10), CSBaseLib.PacketDef.PACKET_HEADER_SIZE, 1024);

            IsNetworkThreadRunning = true;
            NetworkReadThread = new System.Threading.Thread(this.NetworkReadProcess);
            NetworkReadThread.Start();
            NetworkSendThread = new System.Threading.Thread(this.NetworkSendProcess);
            NetworkSendThread.Start();

            IsMainProcessRunning = true;
            dispatcherUITimer.Tick += new EventHandler(MainProcess);
            dispatcherUITimer.Interval = new TimeSpan(0, 0, 0, 0, 100);
            dispatcherUITimer.Start();
        }

        void MainProcess(object sender, EventArgs e)
        {
            ProcessLog();

            try
            {
                var packet = new PacketData();

                lock (((System.Collections.ICollection)RecvPacketQueue).SyncRoot)
                {
                    if (RecvPacketQueue.Count() > 0)
                    {
                        packet = RecvPacketQueue.Dequeue();
                    }
                }

                if (packet.PacketID != 0)
                {
                    PacketProcess(packet);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("ReadPacketQueueProcess. error:{0}", ex.Message));
            }
        }

        private void ProcessLog()
        {
            // 로그 작업만 너무 길게 하면 안되므로 제한
            int logWorkCount = 0;

            while (IsMainProcessRunning)
            {
                System.Threading.Thread.Sleep(1);

                string msg;

                if (Log.GetLog(out msg))
                {
                    ++logWorkCount;

                    if (listBoxLog.Items.Count > 512)
                    {
                        listBoxLog.Items.Clear();
                    }

                    listBoxLog.Items.Add(msg);
                    var lastIndex = listBoxLog.Items.Count - 1;
                    listBoxLog.ScrollIntoView(listBoxLog.Items[lastIndex]);
                }
                else
                {
                    break;
                }

                if (logWorkCount > 10)
                {
                    break;
                }
            }
        }

        void NetworkReadProcess()
        {
            const Int16 PacketHeaderSize = CSBaseLib.PacketDef.PACKET_HEADER_SIZE;

            while (IsNetworkThreadRunning)
            {
                if (!Network.IsConnected())
                {
                    System.Threading.Thread.Sleep(1);
                    continue;
                }

                var recvData = Network.Receive();

                if (recvData != null)
                {
                    PacketBuffer.Write(recvData.Item2, 0, recvData.Item1);

                    while (true)
                    {
                        var data = PacketBuffer.Read();
                        if (data.Count < 1)
                        {
                            break;
                        }

                        var packet = new PacketData();
                        packet.DataSize = (short)(data.Count - PacketHeaderSize);
                        packet.PacketID = BitConverter.ToInt16(data.Array, data.Offset + 2);
                        packet.Type = (SByte)data.Array[(data.Offset + 4)];
                        packet.BodyData = new byte[packet.DataSize];
                        Buffer.BlockCopy(data.Array, (data.Offset + PacketHeaderSize), packet.BodyData, 0, (data.Count - PacketHeaderSize));
                        lock (((System.Collections.ICollection)RecvPacketQueue).SyncRoot)
                        {
                            RecvPacketQueue.Enqueue(packet);
                        }
                    }

                    Log.Write($"받은 데이터: {recvData.Item2}", LOG_LEVEL.INFO);
                }
                else
                {
                    Network.Close();
                    SetDisconnectd();
                    Log.Write("서버와 접속 종료", LOG_LEVEL.INFO);
                }
            }
        }

        void NetworkSendProcess()
        {
            while (IsNetworkThreadRunning)
            {
                System.Threading.Thread.Sleep(1);

                if (!Network.IsConnected())
                {
                    continue;
                }

                lock (((System.Collections.ICollection)SendPacketQueue).SyncRoot)
                {
                    if (SendPacketQueue.Count > 0)
                    {
                        var packet = SendPacketQueue.Dequeue();
                        Network.Send(packet);
                    }
                }
            }
        }

        public void SetDisconnectd()
        {
            ClientState = CLIENT_STATE.NONE;

            SendPacketQueue.Clear();
        }

        void RequestEcho(string message)
        {
            var body = message.ToByteArray();

            Log.Write($"서버 Echo 요청. BodySize:{body.Length}", LOG_LEVEL.INFO);

            var sendData = CSBaseLib.PacketToBytes.Make(CSBaseLib.PACKETID.REQ_RES_TEST_ECHO, body);
            PostSendPacket(sendData);
        }

        public void PostSendPacket(byte[] sendData)
        {
            if (!Network.IsConnected())
            {
                Log.Write("서버와 연결이 되어 있지 않습니다", LOG_LEVEL.ERROR);
                return;
            }

            SendPacketQueue.Enqueue(sendData);
        }

        void PacketProcess(PacketData packet)
        {
            switch ((PACKETID)packet.PacketID)
            {
                case PACKETID.REQ_RES_TEST_ECHO:
                    {
                        Log.Write($"Echo 응답: {packet.BodyData.Length}", LOG_LEVEL.INFO);
                        break;
                    }
            }
        }

        // 접속
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string address = textBoxIP.Text;

            int port = Convert.ToInt32(textBoxPort.Text);

            Log.Write($"서버에 접속 시도: ip:{address}, port:{port}", LOG_LEVEL.INFO);

            if (Network.Connect(address, port))
            {
                labelConnState.Content = string.Format("{0}. 서버 접속 중", DateTime.Now);
                ClientState = CLIENT_STATE.CONNECTED;
            }
            else
            {
                labelConnState.Content = string.Format("{0}. 서버 접속 실패", DateTime.Now);
            }
        }

        // 접속 끊기
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            ClientState = CLIENT_STATE.NONE;
            SetDisconnectd();
            Network.Close();

            labelConnState.Content = string.Format("{0}. 서버 접속 종료", DateTime.Now);
        }

        private void Button_Click_Echo(object sender, RoutedEventArgs e)
        {
            RequestEcho(textBoxEcho.Text);
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Network.Close();

            IsNetworkThreadRunning = false;
            IsMainProcessRunning = false;

            if (NetworkReadThread.IsAlive)
            {
                NetworkReadThread.Join();
            }

            if (NetworkSendThread.IsAlive)
            {
                NetworkSendThread.Join();
            }
        }
    }
}
