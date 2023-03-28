using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MessagePack;

using CSBaseLib;

namespace ChatServer
{
    public class PacketHandler
    {
        protected MainServer MainServer;
        protected UserManager UserManager;

        public void Init(MainServer _mainServer, UserManager _userManager)
        {
            MainServer = _mainServer;
            UserManager = _userManager;
        }
    }
}
