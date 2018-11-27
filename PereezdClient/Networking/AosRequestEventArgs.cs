using PereezdClient.Networking.Protocol;
using System;

namespace PereezdClient.Networking
{
    public class AosRequestEventArgs : EventArgs
    {
        public AosCommand AosRequest { get; private set; }

        public AosRequestEventArgs(AosCommand aosRequest)
        {
            AosRequest = aosRequest;
        }
    }
}