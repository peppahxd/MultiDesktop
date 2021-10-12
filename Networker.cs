using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace MultiDesktop
{
    public class Networker
    {
        public TcpClient tcpClient { get; set; }

        private NetworkStream ns { get; set; }

        public List<string> msgsToServer { get; set; }

        public void Init()
        {
            tcpClient = new TcpClient();
            tcpClient.Connect("192.168.0.102", 9090);
            ns = tcpClient.GetStream();           

            msgsToServer = new List<string>();        
        }

        public void Update()
        {
            if (msgsToServer.Count == 0)
                return;

            
            byte[] buffer = new byte[1024];
            buffer = Encoding.ASCII.GetBytes(msgsToServer[0]);
            ns.Write(buffer, 0, buffer.Length);

            msgsToServer.RemoveAt(0);

        }

        
        
    }
}
