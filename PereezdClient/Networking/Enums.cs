using System;

namespace PereezdClient.Networking
{
    [Flags]
    public enum ClientStatus
    {
        Connected,
        PortNotFree,
        Disconnected
    }
}
