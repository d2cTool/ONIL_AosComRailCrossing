using System;

namespace PereezdClient.Networking
{
    public class StatusChangedEventArgs : EventArgs
    {
        public ClientStatus TcpListenerStatus { get; private set; }
        public StatusChangedEventArgs(ClientStatus tcpListenerStatus)
        {
            TcpListenerStatus = tcpListenerStatus;
        }
    }
}