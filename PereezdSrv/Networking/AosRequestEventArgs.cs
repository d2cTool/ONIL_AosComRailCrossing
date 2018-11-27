using System;

namespace PereezdSrv.Networking 
{
    public class AosRequestEventArgs : EventArgs 
    {
        public string AosRequest { get; private set; }

        public AosRequestEventArgs(string aosRequest)
        {
            AosRequest = aosRequest;
        }
    }
}