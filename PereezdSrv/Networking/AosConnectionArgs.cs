using System;
using System.Net;

namespace PereezdSrv.Networking
{
    public class AosConnectionArgs : EventArgs
    {
        public IPEndPoint ClientEndPoint { get; private set; }

        public AosConnectionArgs(IPEndPoint clientEndPoint)
        {
            ClientEndPoint = clientEndPoint;
        }
    }
}