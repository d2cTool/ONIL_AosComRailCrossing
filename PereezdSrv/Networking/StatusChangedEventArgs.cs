using System;

namespace PereezdSrv.Networking
{
    public class StatusChangedEventArgs : EventArgs
    {
        public ListenerStatus Status { get; private set; }

        public StatusChangedEventArgs(ListenerStatus status)
        {
            Status = status;
        }
    }
}