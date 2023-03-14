using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CSBaseLib;

namespace ChatServer
{
    public class PacketHandler
    {
        protected MainServer mainServer;
        protected UserManager userManager;

        public void Init(MainServer _mainServer, UserManager _userManager)
        {
            mainServer = _mainServer;
            userManager = _userManager;
        }
    }

    public class PKH_Common : PacketHandler
    {
        public void RegisterPacketHandler(
            Dictionary<int, Action<ServerPacketData>> _packetHandlerMap)
        {

        }
    }
}
