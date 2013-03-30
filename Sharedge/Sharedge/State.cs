using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Threading;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Configuration;
namespace Sharedge
{
    public class State
    {
        public string Side;
        public IPAddress LocalAddress;
        public IPAddress RemoteAddress;
        public int LocalPort;
        public int RemotePort;
        public TransferCenter TransferCenter;
        public IPEndPoint LocalEndpoint {
          get {
            return new IPEndPoint(LocalAddress, LocalPort);
          }
        }
        public IPEndPoint RemoteEndpoint {
          get {
            return new IPEndPoint(RemoteAddress, RemotePort);
          }
        }
        public State() 
        {
          TransferCenter = new TransferCenter(this);
        }
           
    }
}
