using System;

namespace AosComDevice
{
    public class ComMsgEventArgs : EventArgs
    {
        public string Data { get; private set; }
        public ComMsgEventArgs(string data) { Data = data; }
    }
}
