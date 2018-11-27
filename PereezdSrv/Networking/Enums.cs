using System;

namespace PereezdSrv.Networking
{
    [Flags]
    public enum ListenerStatus
    {
        Listening,
        PortNotFree,
        NotListening
    }
}
